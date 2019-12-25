
//display confirmation message if Delete button is clicked.
//if Cancel is clicked, hide the message
function confirmDelete(userId, isDeleteClicked) {

    if (isDeleteClicked) {
        $('#deleteSpan_' + userId).hide();
        $('#deleteConfirmSpan_' + userId).show();
    } else {
        $('#deleteSpan_' + userId).show();
        $('#deleteConfirmSpan_' + userId).hide();
    }
       
}