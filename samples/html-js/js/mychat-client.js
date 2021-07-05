var server = 'ws://localhost:5000';

var WEB_SOCKET = new WebSocket(server + '/chat');

WEB_SOCKET.onopen = function (evt) {
    console.log('Connection open ...');
    $('#msgList').val('Welcome to our chat.');
};

WEB_SOCKET.onmessage = function (evt) {
    console.log('Received Message: ' + evt.data);

    if (evt.data) {
        var content = $('#msgList').val();

        if (content) {
            content = content + '\r\n' + evt.data;
        } else {
            content = content + evt.data;
        }

        $('#msgList').val(content);
    }
};

WEB_SOCKET.onclose = function (evt) {    
    console.log('Connection closed.');
};

$('#btnJoin').on('click', function () {
    var room = $('#txtRoom').val();
    var nick = $('#txtNickName').val();

    if (room && nick) {
        var msg = {
            action: 'join',
            msg: room,
            nick: nick
        };

        $('#msgList').val('');
        WEB_SOCKET.send(JSON.stringify(msg));
    } else {
        var content = $('#msgList').val();
        content = content + '\r\n' + 'Please provide a Room and Nickname.';

        $('#msgList').val(content);        
    }
});

$('#btnSend').on('click', function () {
    var nick = $('#txtNickName').val();
    var message = $('#txtMsg').val();
    var toUser = $('#txtToUser').val();

    if (message) {
        WEB_SOCKET.send(JSON.stringify({
            action: 'send_to_room',
            msg: message,
            nick: nick,
            toUser: toUser
        }));

        $('#txtMsg').val("");
        $('#txtToUser').val("");
    }
});

$('#btnLeave').on('click', function () {
    var nick = $('#txtNickName').val();
    var msg = {
        action: 'leave',
        msg: '',
        nick: nick
    };
    WEB_SOCKET.send(JSON.stringify(msg));
});

$("#txtMsg").keyup(function (event) {
    if (event.keyCode === 13) {
        $("#btnSend").click();
    }
});

$(document).ready(function () {
    $(window).bind("beforeunload", function () {
        $('#btnLeave').click();
    });
});
