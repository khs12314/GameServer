using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SnakeServer
{
    public class Security
    {
        public static string GetSalt()
        {
            byte[] bytes = new byte[32]; // 32바이트 정도면 충분

            
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }

            // DB에 저장하기 편하게 문자열(Base64)로 변환
            return Convert.ToBase64String(bytes);
        }

    
        public static string MakeHash(string plainPassword, string salt)
        {
            // 비번 뒤에 소금을 섞음 (Mixing)
            byte[] bytes = Encoding.UTF8.GetBytes(plainPassword + salt);

            using (var sha = SHA256.Create())
            {
                byte[] hashBytes = sha.ComputeHash(bytes);

                // 역시 DB 저장을 위해 문자열로 변환
                return Convert.ToBase64String(hashBytes);
            }
        }
    }
}
