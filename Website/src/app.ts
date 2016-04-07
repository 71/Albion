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
};
