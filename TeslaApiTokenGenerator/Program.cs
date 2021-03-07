using System;
using System.Threading.Tasks;
using TeslaAuth;

namespace TeslaApiTokenGenerator
{
    internal class Program
    {
        private static async Task Main()
        {
            Tokens tokens;
            var teslaAuthHelper = new TeslaAuthHelper("CustomUserAgent/1.0");
            var useRefreshToken = ChooseToUseRefreshToken();

            if (useRefreshToken)
            {
                Console.Write("Enter refresh token: ");
                var refreshToken = Console.ReadLine();
                tokens = await GetTokens(teslaAuthHelper, refreshToken);
            }
            else
            {
                var (username, password, mfaCode) = GetCredentials();
                tokens = await GetTokens(teslaAuthHelper, username, password, mfaCode);
            }

            Console.WriteLine($"\nAccess token:\n{tokens.AccessToken}");
            Console.WriteLine($"\nRefresh token:\n{tokens.RefreshToken}\n");
            Console.WriteLine("Press ENTER to exit...");
            Console.ReadLine();
        }

        private static bool ChooseToUseRefreshToken()
        {
            bool useRefreshToken;

            while (true)
            {
                Console.Write("Generate new tokens with refresh token (Y/N)? ");

                var answer = Console.ReadLine();

                if (answer != null && answer.Equals("Y", StringComparison.InvariantCultureIgnoreCase))
                {
                    useRefreshToken = true;
                    break;
                }

                if (answer != null && answer.Equals("N", StringComparison.InvariantCultureIgnoreCase))
                {
                    useRefreshToken = false;
                    break;
                }

                Console.Write("Only Y or N are valid answers \n");
            }

            return useRefreshToken;
        }

        private static (string username, string password, string mfaCode) GetCredentials()
        {
            Console.Write("Enter the username/e-mail address of your Tesla account: ");
            var username = Console.ReadLine();

            Console.Write("Enter the password of your Tesla account: ");
            var password = GetMaskedString();

            Console.Write("Enter multi-factor authentication code or leave empty if not applicable: ");
            var mfaCode = GetMaskedString();

            return (username, password, mfaCode);
        }

        private static async Task<Tokens> GetTokens(TeslaAuthHelper teslaAuthHelper, string username, string password, string mfaCode)
        {
            var tokens = string.IsNullOrWhiteSpace(mfaCode)
                ? await teslaAuthHelper.AuthenticateAsync(username, password)
                : await teslaAuthHelper.AuthenticateAsync(username, password, mfaCode);

            return tokens;
        }

        private static async Task<Tokens> GetTokens(TeslaAuthHelper teslaAuthHelper, string refreshToken)
        {
            var tokens = await teslaAuthHelper.RefreshTokenAsync(refreshToken, TeslaAccountRegion.Unknown);
            return tokens;
        }

        public static string GetMaskedString()
        {
            var output = string.Empty;
            var consoleKeyInfo = Console.ReadKey(true);
            while (consoleKeyInfo.Key != ConsoleKey.Enter)
            {
                if (consoleKeyInfo.Key == ConsoleKey.Backspace && !string.IsNullOrEmpty(output))
                {
                    output = HandleBackspaceForMaskedString(output);
                }

                if (consoleKeyInfo.Key != ConsoleKey.Backspace)
                {
                    Console.Write("*");
                    output += consoleKeyInfo.KeyChar;
                }

                consoleKeyInfo = Console.ReadKey(true);
            }

            Console.WriteLine();
            return output;
        }

        private static string HandleBackspaceForMaskedString(string input)
        {
            input = input[..^1];
            var position = Console.CursorLeft;
            Console.SetCursorPosition(position - 1, Console.CursorTop);
            Console.Write(" ");
            Console.SetCursorPosition(position - 1, Console.CursorTop);
            return input;
        }
    }
}