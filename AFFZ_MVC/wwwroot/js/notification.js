const connection = new signalR.HubConnectionBuilder()
    .withUrl("http://192.168.0.115:1501/notificationHub", { withCredentials: true })
    .build();

connection.on("ReceiveNotification", function (message) {
    // Display the notification to the user
    alert(message);
    // Optionally update the UI with the new notification
});

connection.start().catch(function (err) {
    return console.error(err.toString());
});