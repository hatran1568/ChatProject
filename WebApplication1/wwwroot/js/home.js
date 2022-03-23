var createRoomBtn = document.getElementById('create-room-btn')
var createRoomModal = document.getElementById('create-room-modal')
createRoomBtn.addEventListener('click', function () {
    createRoomModal.classList.add('modal-active')
})

function closeModal() {
    createRoomModal.classList.remove('modal-active')
}