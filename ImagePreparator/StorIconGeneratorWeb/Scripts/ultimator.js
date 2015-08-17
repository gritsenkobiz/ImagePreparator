function findPos(obj) {
    var current_left = 0, current_top = 0;
    if (obj.offsetParent) {
        do {
            current_left += obj.offsetLeft;
            current_top += obj.offsetTop;
        } while (obj = obj.offsetParent);
        return { x: current_left, y: current_top };
    }
    return undefined;
}

function rgbToHex(r, g, b) {
    if (r > 255 || g > 255 || b > 255)
        throw "Invalid color component";
    return ((r << 16) | (g << 8) | b).toString(16);
}

function setImageColor(color) {
    $(".img-bg").css("background", color);
}


window.onload = function() {
    var yourImageElement = document.getElementById('original-image');

    if (yourImageElement == null) {
        return;
    }

    var canvas = document.createElement("canvas");
    canvas.width = yourImageElement.width;
    canvas.height = yourImageElement.height;
    canvas.getContext('2d').drawImage(yourImageElement, 0, 0);

    var container = document.getElementById('original-container');
    container.appendChild(canvas);

    container.removeChild(yourImageElement);

    $(canvas).click(function(e) {
        var position = findPos(this);
        var x = e.pageX - position.x;
        var y = e.pageY - position.y;
        var coordinate = "x=" + x + ", y=" + y;
        var canvas = this.getContext('2d');
        var p = canvas.getImageData(x, y, 1, 1).data;
        var hex = "#" + ("000000" + rgbToHex(p[0], p[1], p[2])).slice(-6);
        //                alert("HEX: " + hex);
        $("#color-picker").spectrum({
            color: hex,
            showAlpha: true
        });

        setImageColor(hex);
    });

    $("#color-picker").spectrum({
        showAlpha: true,
        change: function(color) {
            setImageColor(color.toHexString());
        }
    });

}