﻿using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Warzone.Authentication;
using Warzone.Clients;
using Warzone.Exceptions;
using Warzone.Http;
using Warzone.Models;
using Warzone.Models.CodApi;

namespace Warzone
{
    public class WarzoneClient : IWarzoneClient
    {
        private readonly IAuthenticationHandler _authenticationHandler;
        private readonly ICodApiClient _codApiClient;

        public WarzoneClient()
        {
            var httpService = new HttpService();
            _codApiClient = new CodApiClient(httpService);
            _authenticationHandler = new AuthenticationHandler(_codApiClient);
        }

        public Task<bool> LoginAsync(string email, string password) =>
            _authenticationHandler.LoginAsync(email, password);
        
        public async Task<WarzoneResponse<Summaries>> GetLastTwentyWarzoneMatchesAsync(string playerName,
            string platform,
            CancellationToken? cancellationToken = null)
        {
            if (!_authenticationHandler.LoggedIn) throw new NotLoggedInException();

            var result = await _codApiClient.GetLastTwentyWarzoneMatchesAsync(playerName, platform, cancellationToken);

            CheckResultForFailure(result);

            return new WarzoneResponse<Summaries>
            {
                Data = result.Data,
                ErrorMessage = result.Error?.Message
            };
        }

        public async Task<WarzoneResponse<Summaries>> GetWarzoneMatchesAsync(string playerName, string platform, DateTime? startTime = null, DateTime? endTime = null,
            CancellationToken? cancellationToken = null)
        {
            if (!_authenticationHandler.LoggedIn) throw new NotLoggedInException();

            var result = await _codApiClient.GetWarzoneMatchesAsync(playerName, platform, startTime, endTime, cancellationToken);

            CheckResultForFailure(result);

            return new WarzoneResponse<Summaries>
            {
                Data = result.Data,
                ErrorMessage = result.Error?.Message
            };
        }

        private static void CheckResultForFailure<T>(CodApiResponse<T> result)
        {
            if (result.Success) return;

            if (result.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new NotLoggedInException();
            }

            throw new WarzoneException();
        }
    }
}