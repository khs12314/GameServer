using MySqlConnector;
using NetWorkModule;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        public static MySqlConnection? GetConnection()
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
        public static bool CreateAccount(string name, string pw, string NicName)
        {
            // using 문법: 블록 벗어나면 자동으로 conn.Close() 호출 -> 커넥션 풀 반납
            using (MySqlConnection? conn = GetConnection())
            {
                if (conn == null) return false;

                string sql = "INSERT INTO accounts (account_name, password, salt, nickname) VALUES (@name, @pw, @salt, @nickname)";
                string salt = Security.GetSalt();
                pw = Security.MakeHash(pw, salt);
                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    // SQL Injection 방지를 위해 파라미터 바인딩 사용 (@변수)
                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.Parameters.AddWithValue("@pw", pw);
                    cmd.Parameters.AddWithValue("@salt", salt);
                    cmd.Parameters.AddWithValue("@nickname", NicName);
                    // 실행! (영향 받은 행의 수 리턴)
                    int rows = cmd.ExecuteNonQuery();

                    if (rows > 0)
                    {
                        Console.WriteLine($"[DB] 회원가입 성공: {name}");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine($"[DB] 회원가입 실패: {name}");
                        return false;

                    }
                }
            }
        }
        public static async Task SavePlayerToDBAsync(int playerDbId)
        {
            try
            {
                // 1. Redis에서 최신 캐시 데이터(더티 데이터) 긁어오기
                string redisKey = $"player:{playerDbId}:info";
                Console.WriteLine($"진입점");
                // RedisManager._db 객체가 public으로 열려있다고 가정한다.
                var hashEntries = await RedisManager.Db.HashGetAllAsync(redisKey);
                Console.WriteLine($"redis 가져오기");
                // 캐시에 데이터가 없으면 저장할 것도 없으니 종료
                if (hashEntries.Length == 0) return;
                Console.WriteLine($"redis 가져오기 완료");
                // Redis Hash에서 값 추출 (C# Dictionary처럼 캐스팅해서 꺼낸다)
                int hp = (int)hashEntries.FirstOrDefault(e => e.Name == "Hp").Value;
                float posX = (float)hashEntries.FirstOrDefault(e => e.Name == "PosX").Value;
                float posY = (float)hashEntries.FirstOrDefault(e => e.Name == "PosY").Value;
                float posZ = (float)hashEntries.FirstOrDefault(e => e.Name == "PosZ").Value;
                Console.WriteLine($"redis 캐스팅");
                // 2. MySQL에 비동기로 UPDATE 쿼리 때리기 (워커 쓰레드 풀에서 실행되므로 JobQueue 안 막힘)
                using (var conn = new MySqlConnection(connString)) // 풀링된 커넥션 사용
                {
                    await conn.OpenAsync();
                    Console.WriteLine($"db작업 시작");
                    // 자네가 알려준 스키마의 대소문자를 완벽하게 맞춘 쿼리다.
                    string sql = @"
INSERT INTO players (accountid, hp, posx, posy, posz) 
VALUES (@accountid, @hp, @posx, @posy, @posz)
ON DUPLICATE KEY UPDATE 
hp = @hp, posx = @posx, posy = @posy, posz = @posz";

                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        Console.WriteLine($"테이블 접근");
                        // 자네의 C# 변수 이름(playerDbId)을 @accountid 구멍에 꽂아 넣는다.
                        cmd.Parameters.AddWithValue("@accountid", playerDbId);
                        cmd.Parameters.AddWithValue("@hp", hp);
                        cmd.Parameters.AddWithValue("@posx", posX);
                        cmd.Parameters.AddWithValue("@posy", posY);
                        cmd.Parameters.AddWithValue("@posz", posZ);
                        Console.WriteLine($"파라미터 수정 완료");
                        int rowsAffected = await cmd.ExecuteNonQueryAsync();
                        Console.WriteLine($"쿼리 실행 결과{rowsAffected}");
                        if (rowsAffected == 0)
                        {
                            Console.WriteLine($"[DB 경고] 쿼리는 정상 실행됐지만, DB에 {playerDbId}번 유저가 아예 없습니다! (0 Rows Affected)");
                        }
                        else
                        {
                            Console.WriteLine($"[DB Save] Player {playerDbId} 의 최신 상태 MySQL 저장 완료. ({rowsAffected}줄 변경됨)");
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DB Error] 플레이어 상태 저장 실패: {ex.Message}");
            }
        }
        public static async Task<(int dbResult, string nickname)> LoginAsync(string id, string inputPw)
        {

            using (var conn = new MySqlConnection(connString))
            {

                await conn.OpenAsync();
                string sql = "SELECT account_id, password, salt, nickname FROM accounts WHERE account_name = @id";
                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            int account_id = reader.GetInt32("account_id");

                            // 2. DB에서 꺼내오기
                            string dbHash = reader.GetString("password");

                            string dbSalt = reader.GetString("salt");

                            string dbnickname = reader.GetString("nickname");
                            string inputHash = Security.MakeHash(inputPw, dbSalt);

                            if (dbHash == inputHash)
                            {

                                return (account_id, dbnickname);
                            }
                            else
                            {
                                Console.WriteLine($"[Login] 비번 틀림.");
                                return (0, string.Empty);
                            }
                        }
                        else
                        {
                            Console.WriteLine("아이디가 존재하지않음");
                            return (0, string.Empty);
                        }
                    }
                }


            }
        }

        public static int LoginAccount(string id, string inputPw, out string nickname)
        {
            using (MySqlConnection? conn = GetConnection())
            {
                nickname = "";
                if (conn == null) return 0;


                string sql = "SELECT account_id, password, salt, nickname FROM accounts WHERE account_name = @id";

                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);

                    // 조회 결과 읽기 도구 (Reader)
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read()) // 데이터가 존재하면 true
                        {
                            int account_id = reader.GetInt32("account_id");

                            // 2. DB에서 꺼내오기
                            string dbHash = reader.GetString("password");

                            string dbSalt = reader.GetString("salt");

                            string dnnickname = reader.GetString("nickname");

                            // 3. 검증 (여기서 아까 만든 Utils 사용!)
                            string inputHash = Security.MakeHash(inputPw, dbSalt);

                            if (dbHash == inputHash)
                            {
                                nickname = dnnickname;
                                Console.WriteLine($"[Login] 성공! 환영합니다 {nickname}님.리턴되는 값{account_id}");
                                return account_id;
                            }
                            else
                            {
                                Console.WriteLine($"[Login] 비번 틀림.");
                                return 0;
                            }
                        }
                        else
                        {
                            Console.WriteLine($"[Login] 아이디가 존재하지 않음.");
                            return 0;
                        }
                    }
                }
            }
        }
    }
}
