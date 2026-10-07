(function () {
    'use strict';

    document.addEventListener('DOMContentLoaded', function () {

        const container = document.getElementById('itemsContainer');
        const addBtn    = document.getElementById('addItemBtn');

        function getMedicineOptions() {
            const tpl = document.getElementById('medicineOptionsTemplate');
            return tpl ? tpl.innerHTML.trim() : '';
        }

        if (addBtn && container) {
            addBtn.addEventListener('click', function () {
                const index = container.querySelectorAll('.item-row').length;
                const options = getMedicineOptions();

                const row = document.createElement('div');
                row.className = 'row g-2 mb-2 item-row';
                row.innerHTML = `
                    <div class="col-md-5">
                        <select name="Input.Items[${index}].MedicineId" class="form-select">
                            <option value="">-- Select Medicine --</option>
                            ${options}
                        </select>
                    </div>
                    <div class="col-md-2">
                        <input name="Input.Items[${index}].Quantity" type="number"
                               step="0.01" min="0.01" value="1"
                               class="form-control" placeholder="Qty" />
                    </div>
                    <div class="col-md-4">
                        <input name="Input.Items[${index}].Dosage" class="form-control"
                               placeholder="Dosage (e.g. 1+0+1)" />
                    </div>
                    <div class="col-md-1">
                        <button type="button"
                                class="btn btn-outline-danger removeItemBtn">✕</button>
                    </div>
                    <input type="hidden" name="Input.Items[${index}].BatchId" value="" />
                `;
                container.appendChild(row);
                reindex();
            });
        }

        if (container) {
            container.addEventListener('click', function (e) {
                if (e.target.classList.contains('removeItemBtn')) {
                    const row = e.target.closest('.item-row');
                    if (row) { row.remove(); reindex(); }
                }
            });
        }

        function reindex() {
            container.querySelectorAll('.item-row').forEach((row, i) => {
                row.querySelectorAll('select, input').forEach(el => {
                    const n = el.getAttribute('name');
                    if (n) el.setAttribute('name',
                        n.replace(/Input\.Items\[\d+\]/, `Input.Items[${i}]`));
                });
            });
        }

        const mr = document.getElementById('MRNumber');
        if (mr) {
            mr.addEventListener('input', function () {
                this.value = this.value.toUpperCase();
            });
        }
    });
})();