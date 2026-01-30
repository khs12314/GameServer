using System;
using System.Collections.Generic;
using System.Linq;

using System.Text;
using System.Threading.Tasks;

namespace NetWorkModule
{
    class Define
    {
        public static readonly short HEADER_SIZE = 2;
    }
    /// <summary>
    /// [헤더][바디] 구조를 갖는 데이터를 파싱하는 크랠스
    /// 헤더: 데이터 사이즈. defines.HEADERSIZE에 정의도니 타입만큼의 크기를 갖는다
    ///       2바이트일 경우  Int16, 4바이트는 Int32로 처리
    /// 바디 : 메시지 본문
    /// </summary>
    class CMessage_resolver
    {
        public delegate void CompltedMessageCallback(Const<byte[]> buffer);

        //메시지 사이즈.
        int message_size;

        //진행 중인 버퍼
        byte[] message_buffer = new byte[4096];

        //현재 진행중인 버퍼의 인덱스를 가리키는 변수.
        //패킷 하나를 완성한 뒤에는 0으로 초기화시켜 줘야한다.
        int current_position;

        //일어와야할 목표 위치.
        int position_to_read;

        //남은 사이즈
        int remain_bytes;

        public CMessage_resolver()
        {
            this.message_size = 0;
            this.current_position = 0;
            this.position_to_read = 0;
            this.remain_bytes = 0;

        }
        /// <summary>
        /// 목표지점으로 설정된 위치까지의 바이트를 원본 버퍼로부터 복사한다.
        /// 데이터가 모자랄 경우 현재 남은 바이트까지만 복샇나다
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="transffered"></param>
        /// <param name="size_to_read"></param>
        /// <returns>다 읽었으면 true, 데이터가 모자라서 못 읽었으면 flase를 리턴한다</returns>

        //1)최초로 헤더를 읽는다
        //2)헤더를 다 읽지 못했다면 수신된 바이트만큼 복사한 뒤 다음 수신을 기다린다.
        //3)헤더를 다 읽었다면 헤더로부터 데이터 사이즈를 구한 뒤 데이터를 읽는다
        //4)수신된 데이터가 사이즈보다 적게 들어 왔다면 수신된 만큼만 복사한 뒤 다음 수신을 기다린다.
        //5)데이터를 모두 읽었다면 하나의 패킷이 완서된 것으로 보고 1)부터 다시 반복한다.
        
        bool read_until(byte[] buffer, ref int src_position, int offset, int transffered)
        {
            //if (this.current_position >= offset + transffered)
            //{
            //    //들어온 데이터만큼 다 읽은 상태이므로 더 이상 읽을 데이터가 없다.
            //    return false;
            //}

            //읽어와야할 바이트.
            //데이터가 분리되어 올 경우 이전에 읽어 놓은 값을 빼줘서 부족한 만큼
            //읽어올 수 있도록 계산해준다.
            int copy_size = this.position_to_read - this.current_position;

            //남은 데이터가 더 적다면 가능한 만큼만 복사
            if (this.remain_bytes < copy_size)
            {
                copy_size = this.remain_bytes;  

            }
            if (copy_size == 0) return false;
            //Console.WriteLine($"{position_to_read}, {this.current_position}, {copy_size}");
            if(position_to_read < 0 || copy_size < 0)
            {
                Console.WriteLine("정지");
            }
            //버퍼에 복사.
            Array.Copy(buffer, src_position, this.message_buffer, this.current_position, copy_size);

            //원본 버퍼 포지션 이동.
            src_position += copy_size;

            //타겟 버퍼 포지션도 이동
            this.current_position += copy_size;

            //남은 바이트 수
            this.remain_bytes -= copy_size; 

            //목표 지점에 도달 못했으면 false
            if(this.current_position < this.position_to_read)
            {
                return false;
            }
            return true;
        }


        /// <summary>
        /// 소켓 버퍼로 부터 데이터를 수신할 때마다 호출된다.
        /// 데이터가 남아있을 때까지 계속 패킷을 만들어 callback을 호출한다.
        /// 하나의 패킷을 완성하지 못했다면 버퍼에 보관해 놓은 뒤 다음 수신을 기다린다
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="transffered"></param>
        public void on_receive(byte[] buffer, int offset, int transffered, CompltedMessageCallback callback)
        {
            //이번 receive로 읽어오게 될 바이트 수.
            this.remain_bytes = transffered;

            //원본 버퍼의 포지션값
            //패킷이 여러개 뭉쳐올 경우 원본 버퍼의 포지션은 계속 앞으로 가야하는데 그 처리를 위한 변수
            int scr_position = offset;

            //남은 데이터가 있다면 계속 반복
            while (this.remain_bytes > 0)
            {
                bool completed = false;

                //헤더만큼 못 읽은 경우 헤더를 먼저 읽는다.

                if (this.current_position < Define.HEADER_SIZE)
                {

                    //목표 지점 설정(헤더 위치까지 도달하도록 설정
                    this.position_to_read = Define.HEADER_SIZE;
                    completed = read_until(buffer, ref scr_position, offset, transffered);
                    if (!completed)
                    {
                        //아직 다 못 읽었으므로 다음 receive를 기다린다
                        return;
                    }
                    //헤더 하나를 온전히 읽어왔으므로 메시지 사이즈를 구한다.
                    this.message_size = get_body_size();

                    //다음 목표 지점(헤더 + 메시지 사이즈
                    this.position_to_read = this.message_size + Define.HEADER_SIZE;
                }
                //메시지를 읽는다.
                completed = read_until(buffer, ref scr_position, offset, transffered);

                if (completed)
                {
                    //패킷 하나를 완성했다.
                    callback(new Const<byte[]>(this.message_buffer));

                    clear_buffer();
                }
            }

        }
        int get_body_size()
        {
            //헤더에서 메시지 사이즈를 구한다.
            //헤더 타입은 Int16, Int32 두 가지 올 수 있으므로 각각을 구분하여 처리한다.
            //사실 헤더가 바뀔 경우는 없을테니 그냥 한 가지로 고정하는 편이 깔끔할 것 같다.
            Type type = Define.HEADER_SIZE.GetType();
            if (type.Equals(typeof(Int16)))
            {
                
                return BitConverter.ToInt16(this.message_buffer, 0);
            }
            
            return BitConverter.ToInt32(this.message_buffer, 0);
        }
        void clear_buffer()
        {
            //Console.WriteLine($"기존 버퍼 지우기 클리어 버퍼");
            Array.Clear(this.message_buffer, 0, this.message_buffer.Length);

            this.current_position = 0;
            this.message_size = 0;  
        }



    }
}


































