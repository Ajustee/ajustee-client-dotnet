﻿using System.Threading.Tasks;

namespace Ajustee.Tools
{
    class Program
    {
        #region Public methods region

        public static async Task Main(string[] args)
        {
            if (args != null && args.Length > 0 && args[0] == "ws")
                await WebSocketClient.ExecuteAsync();
            else
                await AjusteeClient.ExecuteAsync();
        }

        #endregion
    }
}
