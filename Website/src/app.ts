window.onload = () => {

    // miscellaneous
    function getByClassName(str: string) : Element {
        return document.getElementsByClassName(str)[0];
    }

    function anyOf<T>(...possibilities: T[]) : T {
        let nbr = Math.floor(Math.random() * possibilities.length);
        return possibilities[nbr];
    }

    function choose<T>(...possibilities: [number, T][]) : T {
        let arr = new Array<number>();

        for (let i = 0; i < possibilities.length; i++) {
            for (let o = 0; o < Math.ceil(possibilities[i][0] * 100); o++) {
                arr.push(i);
            }
        }

        return possibilities[arr[Math.floor(Math.random() * arr.length)]][1];
    }

    function between(min, max: number) : number {
        return min + Math.floor(max * Math.random());
    }

    function scrollTo(scrollTargetY: number = 0, speed: number = 2000, easing: string = 'easeOutSine') : void {
        let scrollY = window.scrollY,
            currentTime = 0;

        // min time .1, max time .8 seconds
        let time = Math.max(.1, Math.min(Math.abs(scrollY - scrollTargetY) / speed, .8));

        // easing equations from https://github.com/danro/easing-js/blob/master/easing.js
        let PI_D2 = Math.PI / 2,
            easingEquations = {
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

        // add animation loop
        function tick() {
            currentTime += 1 / 60;

            let p = currentTime / time;
            let t = easingEquations[easing](p);

            if (p < 1) {
                requestAnimationFrame(tick);
                window.scrollTo(0, scrollY + ((scrollTargetY - scrollY) * t));
            } else {
                window.scrollTo(0, scrollTargetY);
            }
        }

        // call it once to get started
        tick();
    }

    function fill(i: number) {
        const input = document.getElementById('exin') as HTMLInputElement;
        const values = ['Remind me to eat chocolate', 'Call mom', 'What\'s one plus one?', 'Who are you?'];

        if (i == values.length) i = 0;

        var ch: number = 0;
        var interval: number;

        interval = setInterval(() => {
            if (input.value.length < values[i].length)
                input.value += values[i][ch++];
            else {
                clearInterval(interval);

                setTimeout(() => {
                    interval = setInterval(() => {
                        if (input.value.length > 0)
                            input.value = input.value.substr(0, input.value.length - 1);
                        else {
                            clearInterval(interval);

                            setTimeout(() => {
                                fill(++i);
                            }, 500);
                        }
                    }, 80);
                }, 1500);
            }
        }, 80);
    }

    fill(0);

    // watch inputs
    const nameinputs = document.querySelectorAll('input.name');
    const namespaces = document.querySelectorAll('span.spaces');
    const appinput = document.querySelector('input.app') as HTMLInputElement;
    const appspaces = document.querySelectorAll('span.app');

    let myName = location.hash.length <= 1 ? 'Greg' : location.hash.substr(1);

    appinput.onblur = (e) => {
        if (appinput.value == "") {
            appinput.value = "Albion";

            for (let i = 0; i < appspaces.length; i++) {
                let ai = appspaces[i] as HTMLSpanElement;

                ai.innerText = appinput.value;
            }
        }
    };

    appinput.oninput = (e) => {
        for (let i = 0; i < appspaces.length; i++) {
            let ai = appspaces[i] as HTMLSpanElement;

            ai.innerText = appinput.value;
        }
    };

    appinput.value = "Albion";

    for (let i = 0; i < nameinputs.length; i++) {
        let ni = nameinputs[i] as HTMLInputElement;

        ni.onblur = (e) => {
            if (ni.value == "") {
                for (let o = 0; o < nameinputs.length; o++) {
                    let no = nameinputs[o] as HTMLInputElement;
                    let ns = namespaces[o] as HTMLSpanElement;

                    ns.innerText = '';
                    for (let p = 0; p < myName.length; p++)
                        ns.innerText += ' ';

                    no.style.width = (myName.length / 1.75) + 'em';
                    no.value = myName;
                }
            }
        };

        ni.oninput = (e) => {
            for (let o = 0; o < namespaces.length; o++) {
                let no = namespaces[o] as HTMLSpanElement;

                no.innerText = '';
                for (let p = 0; p < ni.value.length; p++)
                    no.innerText += ' ';
            }

            for (let o = 0; o < nameinputs.length; o++) {
                let no = nameinputs[o] as HTMLInputElement;

                no.style.width = (ni.value.length / 1.75) + 'em';

                if (o != i)
                    no.value = ni.value;
            }
        };

        ni.value = myName;
        ni.style.width = (ni.value.length / 1.75) + 'em';

        for (let o = 0; o < namespaces.length; o++) {
            let no = namespaces[o] as HTMLSpanElement;

            no.innerText = '';
            for (let p = 0; p < ni.value.length; p++)
                no.innerText += ' ';
        }
    }
};
