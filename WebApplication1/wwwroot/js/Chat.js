var renameRoomBtn = document.getElementById('rename-room-btn')
var renameRoomModal = document.getElementById('rename-room-modal')
renameRoomBtn.addEventListener('click', function () {
    renameRoomModal.classList.add('modal-active')
    event.preventDefault();
})

function closeRenameModal() {
    renameRoomModal.classList.remove('modal-active')
}

var leaveRoomBtn = document.getElementById('leave-room-btn')
var leaveRoomModal = document.getElementById('leave-room-modal')
leaveRoomBtn.addEventListener('click', function () {
    leaveRoomModal.classList.add('modal-active')
    event.preventDefault();
})

function closeLeaveModal() {
    leaveRoomModal.classList.remove('modal-active')
}