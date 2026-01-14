using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnakeServer
{
    //디비 관리자
    public class DBMananger
    {
        // [접속 정보] 본인 MySQL 비번으로 수정 필수!
        // Pooling=true: 커넥션 풀 사용 (매번 연결/해제하면 서버 터짐. 미리 만들어둔 거 재사용)
        static string connString = "Server=127.0.0.1;Port=3306;Database=game_db;Uid=root;Pwd=1234;Pooling=true;MinPoolSize=10;MaxPoolSize=100;";

        // 1. 커넥션 풀에서 연결 객체 하나 꺼내오기
        public static MySqlConnection GetConnection()
        {
            try
            {
                MySqlConnection conn = new MySqlConnection(connString);
                conn.Open();
                return conn;
            }
            catch (Exception e)
            {
                Console.WriteLine($"[DB Error] {e.Message}");
                return null;
            }
        }

        // [예제 1] 회원가입 (INSERT) - 결과가 필요 없는 쿼리
        public static void CreateAccount(string name, string pw)
        {
            // using 문법: 블록 벗어나면 자동으로 conn.Close() 호출 -> 커넥션 풀 반납
            using (MySqlConnection conn = GetConnection())
            {
                if (conn == null) return;

                string sql = "INSERT INTO accounts (account_name, password) VALUES (@name, @pw)";

                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    // SQL Injection 방지를 위해 파라미터 바인딩 사용 (@변수)
                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.Parameters.AddWithValue("@pw", pw);

                    // 실행! (영향 받은 행의 수 리턴)
                    int rows = cmd.ExecuteNonQuery();

                    if (rows > 0) Console.WriteLine($"[DB] 회원가입 성공: {name}");
                }
            }
        }

        // [예제 2] 로그인 (SELECT) - 결과(ID)를 가져와야 하는 쿼리
        public static int LoginAccount(string name, string pw)
        {
            using (MySqlConnection conn = GetConnection())
            {
                if (conn == null) return -1;

                // 아이디와 비번이 일치하는 녀석의 'account_id'를 가져와라
                string sql = "SELECT account_id FROM accounts WHERE account_name = @name AND password = @pw";

                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.Parameters.AddWithValue("@pw", pw);

                    // 결과가 1개(단일 값)일 때 사용하는 함수
                    object result = cmd.ExecuteScalar();

                    if (result != null)
                    {
                        Console.WriteLine($"[DB] 로그인 성공! AccountID: {result}");
                        return Convert.ToInt32(result);
                    }
                    else
                    {
                        Console.WriteLine("[DB] 로그인 실패 (아이디 없음 or 비번 틀림)");
                        return -1;
                    }
                }
            }
        }
    }
}
