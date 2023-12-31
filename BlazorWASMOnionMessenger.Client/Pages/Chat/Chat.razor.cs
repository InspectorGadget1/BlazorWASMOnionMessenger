﻿using BlazorWASMOnionMessenger.Client.Features.Chats;
using BlazorWASMOnionMessenger.Client.Features.Common;
using BlazorWASMOnionMessenger.Client.Features.Messages;
using BlazorWASMOnionMessenger.Client.Features.Participants;
using BlazorWASMOnionMessenger.Client.Features.Users;
using BlazorWASMOnionMessenger.Client.WebRtc;
using BlazorWASMOnionMessenger.Domain.DTOs.Chat;
using BlazorWASMOnionMessenger.Domain.DTOs.Message;
using BlazorWASMOnionMessenger.Domain.DTOs.Participant;
using BlazorWASMOnionMessenger.Domain.DTOs.User;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Radzen;
using System;

namespace BlazorWASMOnionMessenger.Client.Pages.Chat
{
    public partial class Chat : IDisposable
    {
        [CascadingParameter]
        private Task<AuthenticationState> AuthenticationStateTask { get; set; }
        [Inject]
        protected ContextMenuService ContextMenuService { get; set; }
        [Inject]
        private ISignalRMessageService SignalRMessageService { get; set; }
        [Inject]
        private IMessageService MessageService { get; set; }
        [Inject]
        private IChatService ChatService { get; set; }
        [Inject]
        protected IJSRuntime JSRuntime { get; set; }
        [Inject]
        private IParticipantService ParticipantService { get; set; }
        [Inject]
        private IUserService UserService { get; set; }
        [Inject]
        protected DialogService DialogService { get; set; }


        [Parameter]
        public string ChatId { get; set; } = string.Empty;
        protected List<MessageDto> Messages { get; set; } = new List<MessageDto>();
        protected CreateMessageDto NewMessage { get; set; } = new CreateMessageDto();
        protected string userId { get; set; } = string.Empty;
        private const int skip = 0;
        private const int quantity = 30;
        protected string MessageContainerClass(string senderId) =>
        senderId == userId ? "float-right" : "";
        protected ElementReference messagesContainerRef;
        protected bool isUpdating = false;
        protected MessageDto messageToUpdate = new();
        protected string Search { get; set; } = string.Empty;
        protected List<UserDto> Users { get; set; } = new List<UserDto>();
        protected List<ParticipantDto> Participants { get; set; } = new List<ParticipantDto>();
        protected ChatDto ChatDto { get; set; } = new ChatDto();


        protected override async Task OnInitializedAsync()
        {
            SignalRMessageService.SubscribeToReceiveMessage(HendleReceiveMessage);
            SignalRMessageService.SubscribeToUpdateMessage(HandleUpdateMessage);
            SignalRMessageService.SubscribeToDeleteMessage(HandleDeleteMessage);
            var authState = await AuthenticationStateTask;
            var user = authState.User;
            userId = user.FindFirst("nameid").Value;
            ChatDto = await ChatService.GetChat(int.Parse(ChatId));
            Messages = (await MessageService.Get(userId, int.Parse(ChatId), quantity, skip)).ToList();
            Participants = (await ParticipantService.GetParticipants(int.Parse(ChatId))).ToList();
            await LoadUsers();
        }

        protected async Task AddParticipant(UserDto userDto)
        {
            await ParticipantService.CreateParticipant(new Domain.DTOs.Participant.CreateParticipantDto
            {
                ChatId = int.Parse(ChatId),
                UserId = userDto.Id
            });
        }

        protected async Task LoadUsers()
        {
            var result = await UserService.GetPage(1, 40, "", false, Search);
            Users = result.Entities;
        }

        private void HendleReceiveMessage(MessageDto message)
        {
            Messages.Add(message);
            StateHasChanged();
        }
        protected async Task SendMessage()
        {
            NewMessage.ChatId = int.Parse(ChatId); 
            NewMessage.UserId = userId;
            NewMessage.AttachmentUrl = "tmp";
            await SignalRMessageService.SendMessageToChat(NewMessage);
            NewMessage.MessageText = "";
            StateHasChanged();
        }
        protected async Task UpdateMessage()
        {
            messageToUpdate.MessageText = NewMessage.MessageText;
            await SignalRMessageService.UpdateMessageInChat(messageToUpdate);
            NewMessage.MessageText = "";
            isUpdating = false;
            StateHasChanged();
        }
        private void HandleUpdateMessage(MessageDto messageDto)
        {
            if (int.Parse(ChatId) == messageDto.ChatId)
            {
                int index = Messages.FindIndex(m => m.Id == messageDto.Id);

                if (index != -1)
                {
                    Messages[index] = messageDto;
                }
            }
            StateHasChanged();
        }
        protected async Task DeleteMessage(int messageId)
        {
            await SignalRMessageService.DeleteMessageFromChat(Messages.First(m => m.Id == messageId));
        }
        private void HandleDeleteMessage(MessageDto messageDto)
        {
            if (int.Parse(ChatId) == messageDto.ChatId)
            {
                var messageToRemove = Messages.First(m => m.Id == messageDto.Id);
                Messages.Remove(messageToRemove);
            }
            StateHasChanged();
        }
        void ShowContextMenuWithItems(MouseEventArgs args, MessageDto messageDto)
        {
            ContextMenuService.Open(args,
                messageDto.SenderId == userId ? ContextMenuSets.deleteAndEdit : ContextMenuSets.deleteOnly,
                async (menuArgs) => await OnMenuItemClickAsync(menuArgs, messageDto.Id));
        }
        async Task OnMenuItemClickAsync(MenuItemEventArgs args, int messageId)
        {
            switch (args.Value)
            {
                case "edit":
                    isUpdating = true;
                    messageToUpdate = Messages.First(m => m.Id == messageId);
                    NewMessage.MessageText = Messages.First(m => m.Id == messageId).MessageText;
                    StateHasChanged();
                    break;
                case "delete":
                    await DeleteMessage(messageId);
                    break;
            }
            ContextMenuService.Close();
        }
        protected void HandleCall()
        {
            DialogService.OpenAsync<VideoCallDialog>("Video call",
                new Dictionary<string, object>() { { "ChatId", int.Parse(ChatId) } },
                new DialogOptions() { Width = "1320px", CloseDialogOnEsc = false, ShowClose = false, CloseDialogOnOverlayClick = false });
        }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await JSRuntime.InvokeVoidAsync("scrollToBottom", messagesContainerRef);
        }

        public void Dispose()
        {
            SignalRMessageService.UnsubscribeFromUpdateMessage(HendleReceiveMessage);
        }
    }
}
