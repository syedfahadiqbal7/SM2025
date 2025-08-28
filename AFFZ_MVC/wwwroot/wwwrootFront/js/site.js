// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
if ($('.typed').length > 0) {

	document.addEventListener('DOMContentLoaded', function () {
		ityped.init(document.querySelector(".typed"), {
			strings: ['Welcome to Smart Center. Hey, How can we help you. Click on Select A Service Button To Start the process'],
			typeSpeed: 150,  // Speed of typing
			backSpeed: 80,   // Speed of backspacing
			loop: true       // Loops the animation
		});
	});
}