﻿using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace csharp_googleapi
{
    class Program
    {
        static void Main(string[] args)
        {
            new Program().Run().Wait();
        }

        private ICredential GetServiceAccountCredential(string[] scopes)
        {
            using (var stream = new FileStream("service_account_key.json", FileMode.Open, FileAccess.Read))
            {
                return GoogleCredential.FromStream(stream)
                     .CreateScoped(scopes).UnderlyingCredential;
            }
        }

        private Task<UserCredential> GetUserCredential(string[] scopes)
        {
            using (var stream = new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = "token";
                return GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    scopes,
                    "user", CancellationToken.None, new FileDataStore(credPath, true));
            }
        }

        private async Task Run()
        {
            var scopes = new[] { CalendarService.Scope.CalendarReadonly };
            // ICredential credential = GetServiceAccountCredential(scopes);
            ICredential credential = await GetUserCredential(scopes);
            GetCalendar(credential, "(カレンダーID)"); // アクセスしたいカレンダーのID
        }

        private void GetCalendar(ICredential credential, string calendarId)
        {
            var service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Get Calendar Sample",
            });

            EventsResource.ListRequest request = service.Events.List(calendarId);
            request.TimeMin = DateTime.Now;
            request.ShowDeleted = false;
            request.SingleEvents = true;
            request.MaxResults = 10;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

            // List events.
            Events events = request.Execute();
            Console.WriteLine("Upcoming events:");
            if (events.Items != null && events.Items.Count > 0)
            {
                foreach (var eventItem in events.Items)
                {
                    string when = eventItem.Start.DateTime.ToString();
                    if (String.IsNullOrEmpty(when))
                    {
                        when = eventItem.Start.Date;
                    }
                    Console.WriteLine("{0} ({1})", eventItem.Summary, when);
                }
            }
            else
            {
                Console.WriteLine("No upcoming events found.");
            }
        }
    }
}
