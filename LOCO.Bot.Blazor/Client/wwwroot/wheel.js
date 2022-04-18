String.prototype.hashCode = function () {
	var hash = 5381;
	for (i = 0; i < this.length; i++) {
		char = this.charCodeAt(i);
		hash = ((hash << 5) + hash) + char;
		hash = hash & hash;
	}
	return hash;
}

Number.prototype.mod = function (n) {
	return ((this % n) + n) % n;
}


var wheel = {

	timerHandle: 0,
	timerDelay: 33,

	angleCurrent: 0,
	angleDelta: 0,

	size: 290,

	canvasContext: null,

	colors: [],
	segments: [],

	seg_colors: [],

	maxSpeed: Math.PI / 16,

	upTime: 1000,
	downTime: 17000,

	spinStart: 0,

	frames: 0,

	centerX: 300,
	centerY: 300,

	spin: function () {
		if (wheel.timerHandle == 0) {
			wheel.spinStart = new Date().getTime();
			wheel.maxSpeed = Math.PI / (16 + Math.random());
			wheel.frames = 0;
			wheel.sound.play();

			wheel.timerHandle = setInterval(wheel.onTimerTick, wheel.timerDelay);
		}
	},

	onTimerTick: function () {

		wheel.frames++;

		wheel.draw();

		var duration = (new Date().getTime() - wheel.spinStart);
		var progress = 0;
		var finished = false;

		if (duration < wheel.upTime) {
			progress = duration / wheel.upTime;
			wheel.angleDelta = wheel.maxSpeed
				* Math.sin(progress * Math.PI / 2);
		} else {
			progress = duration / wheel.downTime;
			wheel.angleDelta = wheel.maxSpeed
				* Math.sin(progress * Math.PI / 2 + Math.PI / 2);
			if (progress >= 1)
				finished = true;
		}

		wheel.angleCurrent += wheel.angleDelta;
		while (wheel.angleCurrent >= Math.PI * 2)
			wheel.angleCurrent -= Math.PI * 2;

		if (finished) {
			clearInterval(wheel.timerHandle);
			wheel.timerHandle = 0;
			wheel.angleDelta = 0;

			$("#counter").html("");
		}
	},

	init: function (optionList) {
		try {
			wheel.initAudio();
			wheel.initCanvas();
			wheel.draw();

			$.extend(wheel, optionList);

		} catch (exceptionData) {
			alert('Wheel is not loaded ' + exceptionData);
		}

	},

	initAudio: function () {
		var sound = document.createElement('audio');
		sound.setAttribute('src', 'wheel.mp3');
		wheel.sound = sound;
	},

	initCanvas: function () {
		var canvas = $('#wheel #canvas').get(0);

		if ($.browser.msie) {
			canvas = document.createElement('canvas');
			$(canvas).attr('width', 1000).attr('height', 600).attr('id', 'canvas').appendTo('.wheel');
			canvas = G_vmlCanvasManager.initElement(canvas);
		}

		canvas.addEventListener("click", wheel.spin, false);
		wheel.canvasContext = canvas.getContext("2d");
	},

	update: function () {
		var r = 0;
		wheel.angleCurrent = ((r + 0.5) / wheel.segments.length) * Math.PI * 2;

		var segments = wheel.segments;
		var len = segments.length;
		var colors = wheel.colors;
		var colorLen = colors.length;
		wheel.seg_color = colors;

		wheel.draw();
	},

	draw: function () {
		wheel.clear();
		wheel.drawWheel();
		wheel.drawNeedle();
	},

	clear: function () {
		var ctx = wheel.canvasContext;
		ctx.clearRect(0, 0, 1000, 800);
	},

	drawNeedle: function () {
		var ctx = wheel.canvasContext;
		var centerX = wheel.centerX;
		var centerY = wheel.centerY;
		var size = wheel.size;

		ctx.lineWidth = 1;
		ctx.strokeStyle = '#000000';
		ctx.fileStyle = '#ffffff';

		ctx.beginPath();

		ctx.moveTo(centerX + size - 40, centerY);
		ctx.lineTo(centerX + size + 20, centerY - 10);
		ctx.lineTo(centerX + size + 20, centerY + 10);
		ctx.closePath();

		ctx.stroke();
		ctx.fill();

		var i = wheel.segments.length - Math.floor((wheel.angleCurrent / (Math.PI * 2)) * wheel.segments.length) - 1;

		ctx.textAlign = "left";
		ctx.textBaseline = "middle";
		ctx.fillStyle = '#000000';
		ctx.font = "2em Arial";
		ctx.fillText(wheel.segments[i], centerX + size + 25, centerY);
	},

	drawSegment: function (key, lastAngle, angle) {
		var ctx = wheel.canvasContext;
		var centerX = wheel.centerX;
		var centerY = wheel.centerY;
		var size = wheel.size;

		var segments = wheel.segments;
		var len = wheel.segments.length;
		var colors = wheel.seg_color;

		var value = segments[key];

		ctx.save();
		ctx.beginPath();

		ctx.moveTo(centerX, centerY);
		ctx.arc(centerX, centerY, size, lastAngle, angle, false);
		ctx.lineTo(centerX, centerY);

		ctx.clip(); // It would be best to clip, but we can double performance without it
		ctx.closePath();

		ctx.fillStyle = colors[key];
		ctx.fill();
		ctx.stroke();

		ctx.save();
		ctx.translate(centerX, centerY);
		ctx.rotate((lastAngle + angle) / 2);

		ctx.fillStyle = '#000000';
		ctx.fillText(value.substr(0, 20), size / 2 + 20, 0);
		ctx.restore();

		ctx.restore();
	},

	drawWheel: function () {
		var ctx = wheel.canvasContext;

		var angleCurrent = wheel.angleCurrent;
		var lastAngle = angleCurrent;

		var segments = wheel.segments;
		var len = wheel.segments.length;
		var colors = wheel.colors;
		var colorsLen = wheel.colors.length;

		var centerX = wheel.centerX;
		var centerY = wheel.centerY;
		var size = wheel.size;

		var PI2 = Math.PI * 2;

		ctx.lineWidth = 1;
		ctx.strokeStyle = '#000000';
		ctx.textBaseline = "middle";
		ctx.textAlign = "center";
		ctx.font = "1.5em Arial";

		for (var i = 1; i <= len; i++) {
			var angle = PI2 * (i / len) + angleCurrent;
			wheel.drawSegment(i - 1, lastAngle, angle);
			lastAngle = angle;
		}
		// Draw a center circle
		ctx.beginPath();
		ctx.arc(centerX, centerY, 20, 0, PI2, false);
		ctx.closePath();

		ctx.fillStyle = '#ffffff';
		ctx.strokeStyle = '#000000';
		ctx.fill();
		ctx.stroke();

		// Draw outer circle
		ctx.beginPath();
		ctx.arc(centerX, centerY, size, 0, PI2, false);
		ctx.closePath();

		ctx.lineWidth = 10;
		ctx.strokeStyle = '#000000';
		ctx.stroke();
	},
}

function initWheel() {
	wheel.init();

	var colors = new Array();
	var segments = new Array();
	$.each($('#venues input:checked'), function (_key, cbox) {
		segments.push(cbox.value);
		colors.push(rgb2hex($(cbox).css("background-color")));
	});

	wheel.segments = segments;
	wheel.colors = colors;
	wheel.update();

	// Hide the address bar (for mobile devices)!
	setTimeout(function () {
		window.scrollTo(0, 1);
	}, 0);
}

const rgb2hex = (rgb) => `#${rgb.match(/^rgb\((\d+),\s*(\d+),\s*(\d+)\)$/).slice(1).map(n => parseInt(n, 10).toString(16).padStart(2, '0')).join('')}`
