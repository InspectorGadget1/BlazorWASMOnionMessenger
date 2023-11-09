﻿using BlazorWASMOnionMessenger.Client.Features.Users;
using BlazorWASMOnionMessenger.Client.Shared;
using BlazorWASMOnionMessenger.Domain.DTOs.User;
using Microsoft.AspNetCore.Components;

namespace BlazorWASMOnionMessenger.Client.Pages.User
{
    public partial class Grid : BaseGrid<UserDto>
    {
        [Inject]
        private IUserService UserService { get; set; } = null!;
        [Inject]
        private NavigationManager NavigationManager { get; set; } = null!;

        protected override async Task OnInitializedAsync()
        {
            if (Page == 0) Page = 1;
            if (PageSize == 0) PageSize = 2;
            PopulateOrderProps();
            if (string.IsNullOrEmpty(OrderBy)) OrderBy = OrderProps[0];
            await Fetch();
        }

        protected override async Task Navigate()
        {
            NavigationManager.NavigateTo($"/users?page={Page}&pageSize={PageSize}&orderBy={OrderBy}&orderType={OrderType}&search={Search}");
            await Fetch();
        }

        private async Task Fetch()
        {
            isLoading = true;
            var result = await UserService.GetPage(Page, PageSize, OrderBy, OrderType, Search);
            if (result.IsSuccessful)
            {
                Items = result.Entities;
                Total = result.Quantity;
            }
            isLoading = false;
            StateHasChanged();
        }
    }
}
