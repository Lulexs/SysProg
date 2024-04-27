document.addEventListener("DOMContentLoaded", function () {
  function queryServer() {
    const inputFields = document.querySelectorAll(".input-field");
    const queryParams = Array.from(inputFields)
      .map((input) => `${input.value}`)
      .join("&");

    const url = `http://127.0.0.1:10889/${queryParams}`;
    console.log(url);
    fetch(url)
      .then((response) => response.text())
      .then((html) => {
        document.getElementById("response").innerHTML = html;
      })
      .catch((error) => console.error("Error querying server:", error));
  }

  const addButton = document.getElementById("addInput");
  if (addButton) {
    addButton.addEventListener("click", addInputField);
  }
  const queryButton = document.getElementById("queryServer");
  if (queryButton) {
    queryButton.addEventListener("click", queryServer);
  }

  function addInputField() {
    const container = document.querySelector(".container");
    const newInputGroup = document.createElement("div");
    newInputGroup.classList.add("input-group");

    const newLabel = document.createElement("label");
    newLabel.textContent = "(query):";
    const newInput = document.createElement("input");
    newInput.setAttribute("type", "text");
    newInput.classList.add("input-field");

    const removeButton = document.createElement("button");
    removeButton.classList.add("removeInput");
    removeButton.textContent = "-";
    removeButton.addEventListener("click", function () {
      container.removeChild(newInputGroup);
    });

    newInputGroup.appendChild(newLabel);
    newInputGroup.appendChild(newInput);
    newInputGroup.appendChild(removeButton);
    container.insertBefore(newInputGroup, addButton);
  }
});
