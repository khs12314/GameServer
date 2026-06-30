using System;
using System.IO;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

public class FileLogger
{
    // 스레드 세이프(Thread-Safe)한 생산자-소비자 큐
    private static readonly BlockingCollection<string> _logQueue = new BlockingCollection<string>();
    private static string _logFilePath;
    private static Task _writeTask;
    private static CancellationTokenSource _cts;

    // 서버 시작(Main) 시점에 단 한 번 호출
    public static void Start()
    {
        Console.WriteLine("log started");
        _logFilePath = $"ServerLog_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
        _cts = new CancellationTokenSource();

        // TaskCreationOptions.LongRunning: 스레드풀의 스레드를 낭비하지 않고, 전용 백그라운드 스레드를 OS로부터 하나 할당받아 무한 루프를 돌림
        _writeTask = Task.Factory.StartNew(ProcessQueue, _cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        Log("================ Logger Started ================");
    }

    // 서버 종료 시점에 호출하여 남은 로그를 모두 쓰고 파일 스트림을 닫음
    public static void Stop()
    {
        Log("================ Logger Stopped ================");
        _logQueue.CompleteAdding(); // 더 이상 로그를 받지 않음
        _cts.Cancel();

        try
        {
            _writeTask?.Wait(); // 파일 쓰기가 완전히 끝날 때까지 대기
        }
        catch (AggregateException) { /* Task 취소에 의한 예외 무시 */ }
    }

    // 게임 로직(Room 등) 어디서든 호출할 메서드 (Zero-Blocking)
    public static void Log(string message)
    {
        if (!_logQueue.IsAddingCompleted)
        {
            // 밀리초(fff) 단위까지 기록하여 패킷 순서와 병목을 추적
            string formattedMsg = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}";
            _logQueue.Add(formattedMsg); // 큐에 넣기만 하고 즉시 반환 (I/O 블로킹 없음)
        }
    }

    // 전담 백그라운드 스레드가 실행할 파일 I/O 무한 루프
    private static void ProcessQueue()
    {
        // 팁: 매번 파일을 열고 닫으면 OS 오버헤드가 극심함. StreamWriter를 계속 열어두는 것이 핵심.
        using (StreamWriter sw = new StreamWriter(_logFilePath, append: true))
        {
            sw.AutoFlush = true; // 로그가 큐에서 빠지는 즉시 파일에 씀 (원한다면 false로 두고 주기적으로 수동 Flush 가능)

            try
            {
                // 큐에 데이터가 들어올 때까지 대기(Wait)하다가, 들어오면 깨어나서 파일에 씀
                foreach (string msg in _logQueue.GetConsumingEnumerable(_cts.Token))
                {
                    sw.WriteLine(msg);
                }
            }
            catch (OperationCanceledException)
            {
                // 서버 종료(_cts.Cancel) 시 발생하는 정상적인 루프 탈출
                _cts.Cancel();
            }
        }
    }
}