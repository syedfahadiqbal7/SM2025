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
            FirstName: {
                required: true
            },
            LastName: {
                required: true
            },
            ProviderName: {
                required: true
            },
            PhoneNumber: {
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
            FirstName: {
                required: "Please enter your First name."
            },
            LastName: {
                required: "Please enter your Last name."
            },
            ProviderName: {
                required: "Please enter your Registered Company Name."
            },
            PhoneNumber: {
                required: "Please enter your Phone Number."
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
