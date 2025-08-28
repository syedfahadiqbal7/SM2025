const connection = new signalR.HubConnectionBuilder()
    .withUrl("/notificationHub")
    .build();

connection.on("ReceiveNotification", function (notification) {
    // Add notification to the notification list
    let notificationHtml = `
        <li class="notification-message">
            <a href="#">
                <div class="media noti-img d-flex">
                    <span class="avatar avatar-sm flex-shrink-0">
                        <img class="avatar-img rounded-circle img-fluid" alt="User Image" src="/assets/img/notifications/avatar-01.jpg">
                    </span>
                    <div class="media-body flex-grow-1">
                        <p class="noti-details">${notification.Message}</p>
                        <p class="noti-time"><span class="notification-time">${new Date(notification.Timestamp).toLocaleTimeString()}</span></p>
                    </div>
                </div>
            </a>
        </li>`;
    document.querySelector('.notification-list').innerHTML += notificationHtml;

    // Update notification count
    const notiCount = document.querySelector('.noti-message');
    notiCount.textContent = parseInt(notiCount.textContent) + 1;
});

connection.start().catch(function (err) {
    return console.error(err.toString());
});