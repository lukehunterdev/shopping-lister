const listWsHandler = {
    _serverCmds: {
        getItems: "getItems",
        getWorkingItems: "getWorkingItems",

        setTicked: "setTicked",
        addWorkingItem: "addWorkingItem",
        createNewItem: "createNewItem",
        cleanList: "cleanList",
    },

    _clientCmds: {
        getItems: "getItems",
        getWorkingItems: "getWorkingItems",

        addWorkingItem: "addWorkingItem",
    },

    _listId: 0,
    _listWs: null,
    _ulId: "theList",
    _ulIdSelector: "#theList",
    _newItemsListId: "newItemsList",
    _newItemsListIdSelector: "#newItemsList",
    _itemTextboxId: "newItemTextbox",
    _itemTextboxIdSelector: "#newItemTextbox",
    _itemCreateId: "createItemButton",
    _itemCreateIdSelector: "#createItemButton",
    _listCleanId: "cleanListButton",
    _listCleanSelector: "#cleanListButton",

    _items: [],
    _workingItems: [],

    init: function (listId) {
        listWsHandler._listId = listId;
        const serverCmds = listWsHandler._serverCmds;
        const listWs = new WebSocket(`/ws/list/${listId}`);
        listWsHandler._listWs = listWs;

        listWs.onopen = (event) => {
            listWs.send(serverCmds.getItems);
            listWs.send(serverCmds.getWorkingItems);
        };

        listWs.onmessage = (event) => {
            const pos = event.data.indexOf("|");
            const command = pos === -1 ? event.data : event.data.substr(0, pos);
            const data = pos === -1 ? null : event.data.substr(pos + 1);

            switch (command) {
            case "ping":
                listWs.send("pong");
                break;

            case serverCmds.getItems:
                listWsHandler._processGetItems(data);
                break;

            case serverCmds.getWorkingItems:
            case serverCmds.cleanList:
                listWsHandler._processGetWorkingItems(data);
                listWsHandler._rebuildList();
                break;

            case serverCmds.setTicked:
                listWsHandler._updateTicked(data);
                break;

            case serverCmds.addWorkingItem:
                listWsHandler._processAddWorkingItem(data);
                listWsHandler._rebuildList();
                break;

            case serverCmds.createNewItem:
                listWsHandler._processCreateNewItem(data);
                listWsHandler._rebuildList();
                break;

            default:
                debugger;
                break;
            }
        };

        listWs.onerror = (event) => {
            alert("There was an error");
            //debugger;
        };

        listWs.onclose = (event) => {
            alert("This socket closed");
            //debugger;
        };

        const newItemClick = () => {
            const itemName = $(listWsHandler._itemTextboxIdSelector).val();
            listWsHandler._listWs.send(`${listWsHandler._serverCmds.createNewItem}|${itemName}`);
            $(listWsHandler._itemTextboxIdSelector).val("");
        }

        $(listWsHandler._listCleanSelector).on("click", listWsHandler._listClean_OnClick);

        $(listWsHandler._itemTextboxIdSelector).on("focus", listWsHandler._newItemTextbox_keypress);
        $(listWsHandler._itemTextboxIdSelector).on("blur", () => { setTimeout("listWsHandler._clearNewItemsList();",100); });
        $(listWsHandler._itemCreateIdSelector).on("click", () => { newItemClick(); });
        //$(listWsHandler._itemTextboxIdSelector).on("keypress", (event) => { if (event.keyCode === 13) newItemClick(); });
        $(listWsHandler._itemTextboxIdSelector).on("input", listWsHandler._newItemTextbox_keypress);
    },

    _listClean_OnClick: function () {
        listWsHandler._listWs.send(`${listWsHandler._serverCmds.cleanList}`);
    },

    _newItemTextbox_keypress: function (event) {
        if (typeof event !== "undefined" && event.keyCode === 13) {
            newItemClick();
            return;
        }

        const val = $(listWsHandler._itemTextboxIdSelector).val();
        listWsHandler._rebuildNewItemsList(val);
        //listWsHandler._listWs.send(`${listWsHandler._serverCmds.cleanList}`);
    },

    _sendSetTicked: function (itemId, isTicked) {
        const params = { "ItemId": itemId, "Ticked": isTicked };
        listWsHandler._listWs.send(`${listWsHandler._serverCmds.setTicked}|${JSON.stringify(params)}`);
    },

    _setTicked: function (itemId, isTicked) {
        const wis = listWsHandler._workingItems;
        for (let i = 0; i < wis.length; i++) {
            const wi = wis[i];
            if (wi.ItemId != itemId) continue;

            wi.Ticked = isTicked === true;
        }
    },

    _sendAddWorkingItem: function (itemId) {
        const params = { "ItemId": itemId };
        listWsHandler._listWs.send(`${listWsHandler._serverCmds.addWorkingItem}|${itemId}`);
    },

    _updateTicked: function (data) {
        const params = JSON.parse(data);
        $(`#itemCheck${params.ItemId}`).prop('checked', params.Ticked);
    },

    _clearList: function() {
        $(listWsHandler._ulIdSelector).empty();
    },

    _rebuildList: function() {
        $(listWsHandler._ulIdSelector).empty();
        listWsHandler._workingItems.forEach(wi => {
            const checkId = `itemCheck${wi.ItemId}`;
            const html = `<li class="form-check"><input class="form-check-input" type="checkbox" value="" ` +
                `id="${checkId}"${wi.Ticked ? " checked" : null}><label class="form-check-label" ` +
                `for="${checkId}">${listWsHandler._getItemName(wi.ItemId)}</label></li>`;
            $(listWsHandler._ulIdSelector).append(html);
            $(`#${checkId}`).change(function() {
                const wiId = parseInt(this.id.substring(9));
                listWsHandler._sendSetTicked(wiId, this.checked);
                listWsHandler._setTicked(wiId, this.checked);
            });
        });
    },

    _clearNewItemsList: function () {
        $(listWsHandler._newItemsListIdSelector).empty();
    },

    _rebuildNewItemsList: function(val) {
        let items = [];

        if (typeof val !== "undefined" && val.length > 0) {
            const valParts = val.toLowerCase().split(" ");
            listWsHandler._items.forEach(item => {
                const nameParts = item.Name.toLowerCase().split(" ");
                let shiz = valParts.every((valPart) => nameParts.some((namePart) => namePart.startsWith(valPart)));
                
                if (shiz === true) items.push(item);
            });
        }
        else {
            items = listWsHandler._items;
        }
        
        $(listWsHandler._newItemsListIdSelector).empty();

        const dasItems = [];

        items.forEach(item => {
            const wis = listWsHandler._workingItems;
            for (let i = 0; i < wis.length; i++) {
                if (wis[i].ItemId == item.Id) return;
            }
            dasItems.push(item);
        }); 

        const compare = (a, b) => {
            if (a.Usage > b.Usage) return -1;
            if (a.Usage < b.Usage) return 1;
            return 0
        };
        dasItems.sort(compare);

        let i = 0;
        const max = 10;
        dasItems.forEach(item => {
            if (i === max) return;
            const html = `<button type="button" onclick="listWsHandler._doAddItem(${item.Id})" class="btn btn-link">${item.Name}</button>`;
            $(listWsHandler._newItemsListIdSelector).append(html);
            i++;
        });
    },

    _doAddItem: function(itemId) {
        listWsHandler._sendAddWorkingItem(itemId);
        $(listWsHandler._itemTextboxIdSelector).val("");
    },

    _getItemName: function(itemId) {
        const arr = listWsHandler._items;

        for (let i = 0; i < arr.length; i++) {
            var item = arr[i];

            if (item.Id !== itemId) continue;

            return item.Name;
        }

        throw "No";
    },

    _processGetItems: function(data) {
        listWsHandler._items = JSON.parse(data);
    },

    _processGetWorkingItems: function(data) {
        listWsHandler._workingItems = JSON.parse(data);
        listWsHandler._reorderWorkingItems();
    },

    _processAddItem: function(data) {
        listWsHandler._items.push(JSON.parse(data));
    },

    _processAddWorkingItem: function(data) {
        listWsHandler._workingItems.push(JSON.parse(data));
        listWsHandler._reorderWorkingItems();
    },

    _processCreateNewItem: function(data) {
        const newItems = JSON.parse(data);

        listWsHandler._items.push(newItems.Item);
        listWsHandler._workingItems.push(newItems.WorkingItem);
        listWsHandler._reorderWorkingItems();
    },

    _reorderWorkingItems: function() {
        const compare = (a, b) => {
            if (a.Ordinal < b.Ordinal) return -1;
            if (a.Ordinal > b.Ordinal) return 1;
            return 0
        };
        listWsHandler._workingItems.sort(compare);
    },
};
