/* Keeps the donation form controls and summary in sync. */
const donationTypeButtons = document.querySelectorAll(".donation-type-btn");
const currencyButtons = document.querySelectorAll(".currency-btn");
const amountButtons = document.querySelectorAll(".amount-btn");

const donationType = document.getElementById("donationType");
const donationCurrency = document.getElementById("donationCurrency");
const donationAmount = document.getElementById("donationAmount");

const customButton = document.getElementById("customButton");
const customContainer = document.getElementById("customAmountContainer");
const customAmount = document.getElementById("customAmount");

const summaryAmount = document.getElementById("summaryAmount");
const summaryType = document.getElementById("summaryType");

const currencySymbols = {
    ZAR: "R",
    USD: "$",
    EUR: "€"
};

function getCurrencySymbol() {
    return currencySymbols[donationCurrency ? donationCurrency.value : "ZAR"] || "R";
}

function updateAmountButtonLabels() {
    const symbol = getCurrencySymbol();

    amountButtons.forEach(button => {
        if (button.dataset.amount) {
            const amount = button.dataset.amount.replace(/\B(?=(\d{3})+(?!\d))/g, " ");
            button.textContent = symbol + amount;
        }
    });
}


if (donationTypeButtons.length > 0) {

    donationTypeButtons.forEach(button => {

        button.addEventListener("click", function () {

            donationTypeButtons.forEach(btn => {
                btn.classList.remove("active");
            });

            this.classList.add("active");

            if (donationType) {
                donationType.value = this.dataset.type;
            }

            if (summaryType) {
                summaryType.textContent = this.dataset.type;
            }

        });

    });

}


if (currencyButtons.length > 0) {

    currencyButtons.forEach(button => {

        button.addEventListener("click", function () {

            currencyButtons.forEach(btn => {
                btn.classList.remove("active");
            });

            this.classList.add("active");

            if (donationCurrency) {
                donationCurrency.value = this.dataset.currency;
            }

            updateAmountButtonLabels();
            updateDonationSummary();

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

                if (customContainer) {
                    customContainer.style.display = "block";
                }

                if (donationAmount) {
                    donationAmount.value = "";
                }

                if (customAmount) {
                    customAmount.focus();
                }

                updateDonationSummary();

            } else {

                if (customContainer) {
                    customContainer.style.display = "none";
                }

                if (donationAmount) {
                    donationAmount.value = this.dataset.amount;
                }

                updateDonationSummary();

            }

        });

    });

}


if (customAmount) {

    customAmount.addEventListener("input", function () {

        if (donationAmount) {
            donationAmount.value = this.value;
        }

        updateDonationSummary();

    });

}


function updateDonationSummary() {

    if (!summaryAmount) {
        return;
    }

    const amount = donationAmount
        ? donationAmount.value
        : "";
    const symbol = getCurrencySymbol();


    if (!amount) {

        summaryAmount.textContent = symbol + "0";

        return;
    }


    summaryAmount.textContent =
        symbol + Number(amount).toLocaleString();

}

updateAmountButtonLabels();