const listEditHandler = {
    listId: 0,
    items: [],
    sharedUsers: [],
    otherUsers: [],

    init: function (listId, items, sharedUsers, otherUsers) {
        this.listId = listId;
        this.items = items;
        this.sharedUsers = sharedUsers;
        this.otherUsers = otherUsers;

        $('#editItemForm').on("submit", (event) => {
            const origName = $('#origItemName').val();
            const newName = $('#itemName').val();

            if (origName !== newName) return;

            event.preventDefault();
        });
    },

    _doEditItem: function (itemId) {
        const items = this.items;
        for (let i = 0; i < items.length; i++) {
            const item = items[i];
            if (item.id !== itemId) continue;

            $('#itemName').val(item.name);
            $('.item-name-value').val(item.name);
            $('#origItemName').val(item.name);
            $('.item-id-value').val(item.id);
            $('.list-id-value').val(this.listId);

            break;
        }

        $('#editItemModal').modal('show');
    },

    _doSharedUser: function (userId) {
        const users = this.sharedUsers;
        for (let i = 0; i < users.length; i++) {
            const user = users[i];
            if (user.id !== userId) continue;

            $('#deleteUserModalLabel').html(`Delete Share for ${user.name}`);
            $('.user-id-value').val(user.id);
            $('.list-id-value').val(this.listId);

            break;
        }

        $('#deleteUserModal').modal('show');
    },
};