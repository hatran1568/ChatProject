var createRoomBtn = document.getElementById('create-room-btn')
var crateRoomModal = document.getElementById('create-room-modal')
createRoomBtn.addEventListener('click', function () {
    crateRoomModal.classList.add('modal-active')
    event.preventDefault();
})

function closeModal() {
    crateRoomModal.classList.remove('modal-active')
}

