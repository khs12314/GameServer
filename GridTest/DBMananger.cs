//using MySqlConnector;
//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace SnakeServer
//{
//    //디비 관리자
//    public class DBMananger
//    {
//        // [접속 정보] 본인 MySQL 비번으로 수정 필수!
//        // Pooling=true: 커넥션 풀 사용 (매번 연결/해제하면 서버 터짐. 미리 만들어둔 거 재사용)
//        static string connString = "Server=127.0.0.1;Port=3306;Database=game_db;Uid=root;Pwd=1234;Pooling=true;MinPoolSize=10;MaxPoolSize=100;";

//        // 1. 커넥션 풀에서 연결 객체 하나 꺼내오기
//        public static MySqlConnection? GetConnection()
//        {
//            try
//            {
//                MySqlConnection conn = new MySqlConnection(connString);
//                conn.Open();
//                return conn;
//            }
//            catch (Exception e)
//            {
//                Console.WriteLine($"[DB Error] {e.Message}");
//                return null;
//            }
//        }

//        // [예제 1] 회원가입 (INSERT) - 결과가 필요 없는 쿼리
//        public static bool CreateAccount(string name, string pw, string NicName)
//        {
//            // using 문법: 블록 벗어나면 자동으로 conn.Close() 호출 -> 커넥션 풀 반납
//            using (MySqlConnection? conn = GetConnection())
//            {
//                if (conn == null) return false;

//                string sql = "INSERT INTO accounts (account_name, password, salt, nickname) VALUES (@name, @pw, @salt, @nickname)";
//                string salt = Security.GetSalt();
//                pw = Security.MakeHash(pw, salt);
//                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
//                {
//                    // SQL Injection 방지를 위해 파라미터 바인딩 사용 (@변수)
//                    cmd.Parameters.AddWithValue("@name", name);
//                    cmd.Parameters.AddWithValue("@pw", pw);
//                    cmd.Parameters.AddWithValue("@salt", salt);
//                    cmd.Parameters.AddWithValue("@nickname", NicName);
//                    // 실행! (영향 받은 행의 수 리턴)
//                    int rows = cmd.ExecuteNonQuery();

//                    if (rows > 0)
//                    {
//                        Console.WriteLine($"[DB] 회원가입 성공: {name}");
//                        return true;
//                    }
//                    else
//                    {
//                        Console.WriteLine($"[DB] 회원가입 실패: {name}");
//                        return false;

//                    }
//                }
//            }
//        }
//        public static async Task<(int dbResult, string nickname)> LoginAsync(string id, string inputPw)
//        {

//            using (var conn = new MySqlConnection(connString))
//            {

//                await conn.OpenAsync();
//                string sql = "SELECT account_id, password, salt, nickname FROM accounts WHERE account_name = @id";
//                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
//                {
//                    cmd.Parameters.AddWithValue("@id", id);
//                    using(var reader = await cmd.ExecuteReaderAsync())
//                    {
//                        if(await reader.ReadAsync())
//                        {
//                            int account_id = reader.GetInt32("account_id");

//                            // 2. DB에서 꺼내오기
//                            string dbHash = reader.GetString("password");

//                            string dbSalt = reader.GetString("salt");

//                            string dbnickname = reader.GetString("nickname");
//                            string inputHash = Security.MakeHash(inputPw, dbSalt);

//                            if (dbHash == inputHash)
//                            {
                               
//                                return (account_id, dbnickname);
//                            }
//                            else
//                            {
//                                Console.WriteLine($"[Login] 비번 틀림.");
//                                return (0, string.Empty);
//                            }
//                        }
//                        else
//                        {
//                            Console.WriteLine("아이디가 존재하지않음");
//                            return(0, string.Empty);
//                        }
//                    }
//                }


//            }
//        }

//        public static int LoginAccount(string id, string inputPw, out string nickname)
//        {
//            using (MySqlConnection? conn = GetConnection())
//            {
//                nickname = "";
//                if (conn == null) return 0;


//                string sql = "SELECT account_id, password, salt, nickname FROM accounts WHERE account_name = @id";

//                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
//                {
//                    cmd.Parameters.AddWithValue("@id", id);

//                    // 조회 결과 읽기 도구 (Reader)
//                    using (MySqlDataReader reader = cmd.ExecuteReader())
//                    {
//                        if (reader.Read()) // 데이터가 존재하면 true
//                        {
//                            int account_id = reader.GetInt32("account_id");

//                            // 2. DB에서 꺼내오기
//                            string dbHash = reader.GetString("password");

//                            string dbSalt = reader.GetString("salt");

//                            string dnnickname = reader.GetString("nickname");

//                            // 3. 검증 (여기서 아까 만든 Utils 사용!)
//                            string inputHash = Security.MakeHash(inputPw, dbSalt);

//                            if (dbHash == inputHash)
//                            {
//                                nickname = dnnickname;
//                                Console.WriteLine($"[Login] 성공! 환영합니다 {nickname}님.리턴되는 값{account_id}");
//                                return account_id;
//                            }
//                            else
//                            {
//                                Console.WriteLine($"[Login] 비번 틀림.");
//                                return 0;
//                            }
//                        }
//                        else
//                        {
//                            Console.WriteLine($"[Login] 아이디가 존재하지 않음.");
//                            return 0;
//                        }
//                    }
//                }
//            }
//        }
//    }
//}
