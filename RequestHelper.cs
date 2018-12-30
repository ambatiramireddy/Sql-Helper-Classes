using AddAppAPI.Controllers;
using AddAppAPI.Enums;
using AddAppAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;

namespace AddAppAPI.Helpers
{
    public class RequestHelper
    {
        private ScreenController screenController;
        private AreaController areaController;
        private CityController cityController;

        public RequestHelper()
        {
            screenController = ApiUtilities.CreateInstance<ScreenController>();
            areaController = ApiUtilities.CreateInstance<AreaController>();
            cityController = ApiUtilities.CreateInstance<CityController>();
        }

        public async Task<ScreenBooking[]> GenerateScreenBookingScheduleAsync(Request request)
        {
            IList<ScreenBooking> bookings = new List<ScreenBooking>();
            List<long> screenIds = await GetScreenIds(request);
            foreach (var screenId in screenIds)
            {
                DateTime scheduledDate = request.ScheduledDate.Date.AddSeconds(request.ScheduledStartTime);
                int scheduledDuration = request.ScheduledDuration;
                if (!request.RecurringMessage.Value)
                {
                    bookings.Add(new ScreenBooking
                    {
                        ScreenId = screenId,
                        RequestId = request.Id,
                        ScheduledDate = scheduledDate,
                        ScheduledStartTime = (int)scheduledDate.TimeOfDay.TotalSeconds,
                        ScheduledDuration = scheduledDuration,
                    });
                }
                else
                {
                    for (int i = 0; i < request.RecurringTimes; i++)
                    {
                        bookings.Add(new ScreenBooking
                        {
                            ScreenId = screenId,
                            RequestId = request.Id,
                            ScheduledDate = scheduledDate,
                            ScheduledStartTime = (int)scheduledDate.TimeOfDay.TotalSeconds,
                            ScheduledDuration = scheduledDuration,
                        });

                        scheduledDate = scheduledDate.AddSeconds((double)request.RecurForEvery);
                    }
                }
            }

            return bookings.ToArray();
        }

        public async Task<List<long>> GetScreenIds(Request request)
        {
            List<long> screeIds = new List<long>();
            RequestTargetName target = (RequestTargetName)Enum.Parse(typeof(RequestTargetName), request.TargetTypeId.ToString());
            switch (target)
            {
                case RequestTargetName.Screen:
                    {
                        screeIds.Add(request.TargetId);
                        break;
                    }
                case RequestTargetName.Area:
                    {
                        await AddAreaScreeIds(screeIds, (int)request.TargetId);
                    }
                    break;
                case RequestTargetName.City:
                    {
                        await AddCityScreeIds(screeIds, (int)request.TargetId);
                    }
                    break;
                case RequestTargetName.State:
                    {
                        await AddStateScreeIds(screeIds, (int)request.TargetId);
                    }
                    break;
                case RequestTargetName.Country:
                    break;
                default:
                    break;
            }

            return screeIds;
        }

        private async Task AddAreaScreeIds(List<long> screeIds, int areaId)
        {
            var screenIds = (NegotiatedContentResult<List<long>>)(await this.screenController.GetScreenIdsByAreaId(areaId));
            screeIds.AddRange(screenIds.Content);
        }

        private async Task AddCityScreeIds(List<long> screeIds, int cityId)
        {
            var areaIds = (NegotiatedContentResult<List<int>>)(await this.areaController.GetAreaIdsByCityId(cityId));
            var tasks = areaIds.Content.Select(async areaId =>
            {
                await AddAreaScreeIds(screeIds, areaId);
            });

            await Task.WhenAll(tasks);
        }

        private async Task AddStateScreeIds(List<long> screeIds, int stateId)
        {
            var cityIds = (NegotiatedContentResult<List<int>>)(await cityController.GetCityIdsByStateId(stateId));
            var tasks = cityIds.Content.Select(async cityId =>
            {
                await AddCityScreeIds(screeIds, cityId);
            });

            await Task.WhenAll(tasks);
        }
    }
}