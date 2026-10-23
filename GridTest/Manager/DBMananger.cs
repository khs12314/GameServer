using GridTest;
using GridTest.Network;
using GridTest.Util;
using MySqlConnector;
using NetWorkModule;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridTest.Manager
{
    //디비 관리자
    public class DBMananger
    {
        // [접속 정보] 본인 MySQL 비번으로 수정 필수!
        // Pooling=true: 커넥션 풀 사용 (매번 연결/해제하면 서버 터짐. 미리 만들어둔 거 재사용)
        // Connection settings are stored in the Windows user environment, not in Git.
        static string connString
        {
            get
            {
                string? value = Environment.GetEnvironmentVariable("GRIDTEST_DB_CONNECTION_STRING");
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new InvalidOperationException("Set the GRIDTEST_DB_CONNECTION_STRING environment variable before using the database.");
                }
                return value;
            }
        }

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
        public static async Task<bool> CreateCharacterAsync(int accountId, string nickname, int characterType)
        {
            if (PacketHandler.USE_DB)
            {
                using (MySqlConnection? conn = GetConnection())
                {
                    if (conn == null) return false;

                    // 연결도 비동기로 열어야 한다. (GetConnection 내부에 이미 Open이 있다면 생략 가능)
                    await conn.OpenAsync();

                    string sql = "INSERT INTO characters (account_id, nickname, character_type, level) VALUES (@accountId, @nickname, @characterType, 1)";
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@accountId", accountId);
                        cmd.Parameters.AddWithValue("@nickname", nickname);
                        cmd.Parameters.AddWithValue("@characterType", characterType);

                        // 핵심 교정: I/O 완료를 스레드 낭비 없이 기다린다 (Non-Blocking)
                        int rows = await cmd.ExecuteNonQueryAsync();

                        if (rows > 0)
                        {
                            Console.WriteLine($"[DB] 캐릭터 생성 성공: {nickname}");
                            return true;
                        }
                        else
                        {
                            Console.WriteLine($"[DB] 캐릭터 생성 실패: {nickname}");
                            return false;
                        }
                    }
                }
            }
            else
            {

                return true;
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
        public static async Task GetCharacterPos(int id, Player player)
        {
            if(!PacketHandler.USE_DB)
            {
                Console.WriteLine("DB 사용 안함");
                return;
            }
            using (var conn = new MySqlConnection(connString))
            {
                await conn.OpenAsync();
                string sql = "SELECT accountid, hp, posx, posy, posz FROM players WHERE accountid = @accountid";
                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@accountid", id);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if(await reader.ReadAsync())
                        {
                            int accountid = reader.GetInt32("accountid");
                            int hp = reader.GetInt32("hp");
                            float posx = reader.GetFloat("posx");
                            float posy = reader.GetFloat("posy");
                            float posz = reader.GetFloat("posz");
                            player.Hp = hp;
                            player.PosX = posx;
                            player.PosY = posy;
                            player.PosZ = posz;
                            Console.WriteLine($"캐릭터 정보 조회 성공 {player.Id} : {player.PosX}, {player.PosY}, {player.PosZ}");
                        }
                        else
                        {
                            Console.WriteLine("존재하지않는 캐릭터입니다.");
                        }
                    }
                }
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
                                GetCharacterInfos(account_id);
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
        public static async Task<List<CharacterInfo>> GetCharacterInfos(int accountId)
        {
            List<CharacterInfo> characterInfos = new List<CharacterInfo>();
            using (MySqlConnection? conn = GetConnection())
            {
                if (conn == null) return characterInfos;
                string sql = "SELECT character_id, nickname, character_type, level FROM characters WHERE account_id = @accountId";
                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@accountId", accountId);
                    using (MySqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        // 2. 한 줄(Row)을 읽어오는 행위도 비동기로!
                        while (await reader.ReadAsync())
                        {
                            CharacterInfo info = new CharacterInfo
                            {
                                // 주의: ReadAsync()가 끝났다는 건 이미 한 줄의 데이터가 
                                // C# 서버 메모리 버퍼로 배달되었다는 뜻이다.
                                // 따라서 값을 꺼내는 GetInt32, GetString은 동기로 써도 전혀 문제없다!
                                CharacterId = reader.GetInt32("character_id"),
                                Nickname = reader.GetString("nickname"),
                                CharacterType = reader.GetInt32("character_type"),
                                Level = reader.GetInt32("level")
                            };
                            characterInfos.Add(info);
                        }
                    }
                }
            }
            //Console.WriteLine($"가져온 캐릭터 수 {characterInfos.Count}");
            return characterInfos;
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
