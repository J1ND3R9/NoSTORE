export const userConnection = new signalR.HubConnectionBuilder()
    .withUrl('/userHub')
    .withAutomaticReconnect()
    .build();