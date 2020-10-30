using BeerliftDashboard.Data;
using BeerliftDashboard.Models;
using IoTEdgeConversationDashboard.Data;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BeerliftDashboard.Pages
{
    public class BottleHolderComponentBase : ComponentBase, IDisposable
    {
        [Inject]
        public IoTHubServiceClientService _ioTHubServiceClientService { get; set; }

        [Inject]
        public TelemetryService _telemetryService { get; set; }

        [Inject]
        public SqliteService _sqliteService { get; set; }

        [Parameter]
        public string deviceId { get; set; }

        [Parameter]
        public string moduleName { get; set; }

        [Parameter]
        public EventCallback<Bottleholder> BottleholderSelectEvent { get; set; }

        public string markedText;

        public List<Bottleholder> Bottleholders { get; set; }

        public Bottleholder selectedBottleHolder = null;

        private BeerliftMessage _beerliftMessage = null;

        protected override void OnInitialized()
        {
            base.OnInitialized();

            _telemetryService.InputMessageReceived += OnInputTelemetryReceived;

            Bottleholders = _sqliteService.GetBottleHolders(deviceId, moduleName);
        }

        void IDisposable.Dispose()
        {
            _telemetryService.InputMessageReceived -= OnInputTelemetryReceived;
        }

        public async Task MarkPosition()
        {
            markedText = string.Empty;

            if (selectedBottleHolder == null)
            {
                return;
            }

            markedText = "marking...";

            await _ioTHubServiceClientService.SendDirectMethod<MarkPositionRequest, MarkPositionResponse>(deviceId, moduleName, "MarkPosition", new MarkPositionRequest { position = selectedBottleHolder.id });

            markedText = $"marked {selectedBottleHolder.id}";
        }

        public void BottleHolderSelected(ChangeEventArgs args)
        {
            selectedBottleHolder = (from x in Bottleholders
                                    where x.id.ToString() == args.Value.ToString()
                                    select x).First();

            BottleholderSelectEvent.InvokeAsync(selectedBottleHolder).Wait();
        }

        private async void OnInputTelemetryReceived(object sender, BeerliftMessage message)
        {
            if (message == null
                    || message.deviceId != deviceId)
            {
                return;
            }

            ProcessChanges(_beerliftMessage, message);

            _beerliftMessage = message;

            await InvokeAsync(() => StateHasChanged());
        }

        private void ProcessChanges(BeerliftMessage beerliftMessage, BeerliftMessage message)
        {
        }
    }
}