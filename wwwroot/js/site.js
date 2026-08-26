const typeButtons = document.querySelectorAll(".donation-type-btn");
const amountButtons = document.querySelectorAll(".amount-btn");

const donationType = document.getElementById("donationType");
const donationAmount = document.getElementById("donationAmount");

const customButton = document.getElementById("customButton");
const customContainer = document.getElementById("customAmountContainer");
const customAmount = document.getElementById("customAmount");

if (typeButtons.length > 0) {
    typeButtons.forEach(button => {
        button.addEventListener("click", function () {

            typeButtons.forEach(btn => {
                btn.classList.remove("active");
            });

            this.classList.add("active");

            donationType.value = this.dataset.type;
        });
    });
}

if (amountButtons.length > 0) {
    amountButtons.forEach(button => {
        button.addEventListener("click", function () {

            amountButtons.forEach(btn => {
                btn.classList.remove("active");
            });

            this.classList.add("active");

            if (this === customButton) {
                customContainer.style.display = "block";
                donationAmount.value = "";
                customAmount.focus();
            } else {
                customContainer.style.display = "none";
                donationAmount.value = this.dataset.amount;
            }
        });
    });
}

if (customAmount) {
    customAmount.addEventListener("input", function () {
        donationAmount.value = this.value;
    });
}

// Page Loader

window.addEventListener("load", function () {
    const loader = document.getElementById("pageLoader");

    if (!loader) {
        return;
    }

    if (sessionStorage.getItem("siteLoaded")) {
        loader.remove();
        return;
    }

    setTimeout(function () {
        loader.classList.add("hidden");
        sessionStorage.setItem("siteLoaded", "true");

        setTimeout(function () {
            loader.remove();
        }, 1000);
    }, 1000);
});