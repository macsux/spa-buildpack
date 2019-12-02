"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/springCloudBus").build();

connection.on("NewMessage", function (type, message) {
    var json = JSON.stringify(message, null, "\t"); // Indented with tab
    
    document.getElementById("messages").innerHTML += "===" + type + "===\n" + json + "\n";
    console.log(type);
    console.log(message);
});

connection.start()
    .catch(function (err) {
    return console.error(err.toString());
});
