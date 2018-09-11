$(function() {
    var left = 0;
    var right = 0;
    //点击知道
    $("#knowBtn").click(function () {
        disappear();
        left++;
        $(this).parent().find("i").css({ "color": "#008a75", "border-color": "#008a75" });//按钮变色
        $("#leftProgress").css({ "background-color": "#008a75", "width": (left / (left + right)) * 100 + "%" });
        $("#rightProgress").css({ "width": (right / (left + right)) * 100 + "%" });
        $(".fontShow").css("display", "block");
        $("#leftShow").text(left);
    });

    //点击不知道
    $("#NotknowBtn").click(function () {
        disappear();
        right++;
        $("#rightProgress").css({ "background-color": "#dd5555", "width": (right / (left + right)) * 100 + "%" });
        $("#leftProgress").css({ "width": (left / (left + right)) * 100 + "%" });
        $(".fontShow").css("display", "block");
        $("#rightShow").text(right);
    });

    function disappear() {
        $("#title").css("display", "none");
    }
})

