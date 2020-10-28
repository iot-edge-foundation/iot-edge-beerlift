using BeerliftDashboard.Data;
using BeerliftDashboard.Models;
using IoTEdgeConversationDashboard.Data;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BeerliftDashboard
{
    public class IndexBase : ComponentBase, IDisposable
    {
        [Inject]
        public SqliteService _sqliteService { get; set; }

        [Inject]
        public SessionService _sessionService { get; set; }

        public string password;

        public string _dbpassword;

        public string message;

        public string deviceId { get; set; }

        public string moduleName { get; set; }

        public string telemetryMessage;

        protected override void OnInitialized()
        {
            message = _sqliteService.GetVersion();

            deviceId = _sqliteService.ReadSetting("deviceId");

            moduleName = _sqliteService.ReadSetting("moduleName");

            _dbpassword = _sqliteService.ReadSetting("password");
        }

        void IDisposable.Dispose()
        {
        }

        private bool CorrectPassword
        {
            get
            {
                return (password == _dbpassword)
                            && (!string.IsNullOrEmpty(password));
            }
        }

        public void Validate()
        {
            _sessionService.Validated = CorrectPassword;

            if (_sessionService.Validated
                    && !string.IsNullOrEmpty(deviceId)
                    && !string.IsNullOrEmpty(moduleName))
            {
                _sqliteService.WriteSetting("deviceId", deviceId);
                _sqliteService.WriteSetting("moduleName", moduleName);

                message = "Validated";
            }
            else
            {
                message = "Unable to validate";
            }
        }
    }
}