<script>
    $(document).ready(function () {
        $("#resetPasswordForm").on("submit", function (e) {
            let valid = true;
            $(".error-message").remove();

            const password = $("#NewPassword").val();
            const confirmPassword = $("#ConfirmPassword").val();

            const passwordRegex = /^(?=.*[A-Z])(?=.*[!@#$&*])(?=.*[0-9])(?=.*[a-z]).{8,}$/;

            if (!password) {
                valid = false;
                $("#NewPassword").after('<span class="error-message text-danger">Password is required.</span>');
            } else if (!passwordRegex.test(password)) {
                valid = false;
                $("#NewPassword").after('<span class="error-message text-danger">Password must be at least 8 characters, include an uppercase letter, a symbol, and a number.</span>');
            }

            if (!confirmPassword) {
                valid = false;
                $("#ConfirmPassword").after('<span class="error-message text-danger">Confirm password is required.</span>');
            } else if (password !== confirmPassword) {
                valid = false;
                $("#ConfirmPassword").after('<span class="error-message text-danger">Passwords do not match.</span>');
            }

            if (!valid) {
                e.preventDefault();
            }
        });

    // Toggle password visibility
    $(".toggle-password").on("click", function () {
            const inputField = $(this).siblings("input");
    const icon = $(this).find("i");

    if (inputField.attr("type") === "password") {
        inputField.attr("type", "text");
    icon.removeClass("feather-eye-off").addClass("feather-eye");
            } else {
        inputField.attr("type", "password");
    icon.removeClass("feather-eye").addClass("feather-eye-off");
            }
        });
    });
</script>
