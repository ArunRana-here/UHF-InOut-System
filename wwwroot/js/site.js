const connection = new signalR.HubConnectionBuilder().withUrl("/rfidhub").build();


connection.start().then(() => {
    console.log("Connected to SignalR");
});

connection.on("AllowedTagsUpdated", function (updatedTags) {
    updateAllowedTagsTable(updatedTags); //  Update tags in real-time
});

function updateAllowedTagsTable(tags) {
    let tableBody = document.querySelector("#allowedTagsTable tbody");
    tableBody.innerHTML = "";
    tags.forEach(tag => {
        let row = `<tr>
                <td style="padding: 10px;">${tag}</td>
                <td style="padding: 10px;">
                    <button onclick="removeTag('${tag}')" class="delete-btn">Remove</button>
                </td>
            </tr>`;
        tableBody.innerHTML += row;
    });
}
