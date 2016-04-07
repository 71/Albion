window.onload = function () {
    function getByClassName(str) {
        return document.getElementsByClassName(str)[0];
    }
    function anyOf() {
        var possibilities = [];
        for (var _i = 0; _i < arguments.length; _i++) {
            possibilities[_i - 0] = arguments[_i];
        }
        var nbr = Math.floor(Math.random() * possibilities.length);
        return possibilities[nbr];
    }
    function choose() {
        var possibilities = [];
        for (var _i = 0; _i < arguments.length; _i++) {
            possibilities[_i - 0] = arguments[_i];
        }
        var arr = new Array();
        for (var i = 0; i < possibilities.length; i++) {
            for (var o = 0; o < Math.ceil(possibilities[i][0] * 100); o++) {
                arr.push(i);
            }
        }
        return possibilities[arr[Math.floor(Math.random() * arr.length)]][1];
    }
    function between(min, max) {
        return min + Math.floor(max * Math.random());
    }
    function scrollTo(scrollTargetY, speed, easing) {
        if (scrollTargetY === void 0) { scrollTargetY = 0; }
        if (speed === void 0) { speed = 2000; }
        if (easing === void 0) { easing = 'easeOutSine'; }
        var scrollY = window.scrollY, currentTime = 0;
        var time = Math.max(.1, Math.min(Math.abs(scrollY - scrollTargetY) / speed, .8));
        var PI_D2 = Math.PI / 2, easingEquations = {
            easeOutSine: function (pos) {
                return Math.sin(pos * (Math.PI / 2));
            },
            easeInOutSine: function (pos) {
                return (-0.5 * (Math.cos(Math.PI * pos) - 1));
            },
            easeInOutQuint: function (pos) {
                if ((pos /= 0.5) < 1) {
                    return 0.5 * Math.pow(pos, 5);
                }
                return 0.5 * (Math.pow((pos - 2), 5) + 2);
            }
        };
        function tick() {
            currentTime += 1 / 60;
            var p = currentTime / time;
            var t = easingEquations[easing](p);
            if (p < 1) {
                requestAnimationFrame(tick);
                window.scrollTo(0, scrollY + ((scrollTargetY - scrollY) * t));
            }
            else {
                window.scrollTo(0, scrollTargetY);
            }
        }
        tick();
    }
    function fill(i) {
        var input = document.getElementById('exin');
        var values = ['Remind me to eat chocolate', 'Call mom', 'What\'s one plus one?', 'Who are you?'];
        if (i == values.length)
            i = 0;
        var ch = 0;
        var interval;
        interval = setInterval(function () {
            if (input.value.length < values[i].length)
                input.value += values[i][ch++];
            else {
                clearInterval(interval);
                setTimeout(function () {
                    interval = setInterval(function () {
                        if (input.value.length > 0)
                            input.value = input.value.substr(0, input.value.length - 1);
                        else {
                            clearInterval(interval);
                            setTimeout(function () {
                                fill(++i);
                            }, 500);
                        }
                    }, 80);
                }, 1500);
            }
        }, 80);
    }
    fill(0);
    var nameinputs = document.querySelectorAll('input.name');
    var namespaces = document.querySelectorAll('span.spaces');
    var appinput = document.querySelector('input.app');
    var appspaces = document.querySelectorAll('span.app');
    var myName = location.hash.length <= 1 ? 'Greg' : location.hash.substr(1);
    appinput.onblur = function (e) {
        if (appinput.value == "") {
            appinput.value = "Albion";
            for (var i = 0; i < appspaces.length; i++) {
                var ai = appspaces[i];
                ai.innerText = appinput.value;
            }
        }
    };
    appinput.oninput = function (e) {
        for (var i = 0; i < appspaces.length; i++) {
            var ai = appspaces[i];
            ai.innerText = appinput.value;
        }
    };
    appinput.value = "Albion";
    var _loop_1 = function(i) {
        var ni = nameinputs[i];
        ni.onblur = function (e) {
            if (ni.value == "") {
                for (var o = 0; o < nameinputs.length; o++) {
                    var no = nameinputs[o];
                    var ns = namespaces[o];
                    ns.innerText = '';
                    for (var p = 0; p < myName.length; p++)
                        ns.innerText += ' ';
                    no.style.width = (myName.length / 1.75) + 'em';
                    no.value = myName;
                }
            }
        };
        ni.oninput = function (e) {
            for (var o = 0; o < namespaces.length; o++) {
                var no = namespaces[o];
                no.innerText = '';
                for (var p = 0; p < ni.value.length; p++)
                    no.innerText += ' ';
            }
            for (var o = 0; o < nameinputs.length; o++) {
                var no = nameinputs[o];
                no.style.width = (ni.value.length / 1.75) + 'em';
                if (o != i)
                    no.value = ni.value;
            }
        };
        ni.value = myName;
        ni.style.width = (ni.value.length / 1.75) + 'em';
        for (var o = 0; o < namespaces.length; o++) {
            var no = namespaces[o];
            no.innerText = '';
            for (var p = 0; p < ni.value.length; p++)
                no.innerText += ' ';
        }
    };
    for (var i = 0; i < nameinputs.length; i++) {
        _loop_1(i);
    }
};
