$(document).ready(function () {
    // Password complexity validation
    $.validator.addMethod("passwordComplexity", function (value, element) {
        return this.optional(element) ||
            /[A-Z]/.test(value) && // At least one uppercase letter
            /[0-9]/.test(value) && // At least one number
            /[!@#$%^&*()_\-=\+\[\]{}|;:,.<>?]/.test(value) && // At least one special character
            value.length >= 8 && value.length <= 12; // Length between 8-12 characters
    }, "Password must be 8-12 characters long, include at least one uppercase letter, one number, and one special character.");

    // Apply validation to the form
    $("#signUpForm").validate({
        rules: {
            CustomerName: {
                required: true
            },
            Email: {
                required: true,
                email: true
            },
            Password: {
                required: true,
                passwordComplexity: true
            }
        },
        messages: {
            CustomerName: {
                required: "Please enter your name."
            },
            Email: {
                required: "Please enter your email address.",
                email: "Please enter a valid email address."
            },
            Password: {
                required: "Please enter your password."
            }
        }
    });
});
