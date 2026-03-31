(() => {
  const TAX_RATE = 0.1;

  const MENU_ITEMS = [
    { id: 'C101', name: 'ホットコーヒー', price: 350, category: 'DRINK', tag: 'HOT' },
    { id: 'C102', name: 'アイスコーヒー', price: 380, category: 'DRINK', tag: 'ICE' },
    { id: 'C103', name: 'カフェラテ', price: 420, category: 'DRINK', tag: 'MILK' },
    { id: 'C201', name: 'オレンジジュース', price: 390, category: 'DRINK', tag: 'FRUIT' },
    { id: 'F101', name: 'クロワッサン', price: 280, category: 'FOOD', tag: 'BAKERY' },
    { id: 'F102', name: 'トーストセット', price: 520, category: 'FOOD', tag: 'MORNING' },
    { id: 'F103', name: 'サンドイッチ', price: 560, category: 'FOOD', tag: 'LUNCH' },
    { id: 'F201', name: 'フレンチトースト', price: 640, category: 'FOOD', tag: 'SWEETS' },
    { id: 'S101', name: 'チーズケーキ', price: 480, category: 'SWEETS', tag: 'ケーキ' },
    { id: 'S102', name: 'ガトーショコラ', price: 520, category: 'SWEETS', tag: 'ケーキ' },
    { id: 'S103', name: 'プリン', price: 420, category: 'SWEETS', tag: 'デザート' },
    { id: 'S201', name: 'アイスクリーム', price: 360, category: 'SWEETS', tag: 'COLD' },
  ];

  const CATEGORIES = [
    { id: 'ALL', label: 'すべて', icon: '◎' },
    { id: 'DRINK', label: 'ドリンク', icon: '☕' },
    { id: 'FOOD', label: 'フード', icon: '🍞' },
    { id: 'SWEETS', label: 'スイーツ', icon: '🍰' },
  ];

  const state = {
    currentCategory: 'ALL',
    search: '',
    orderItems: [],
    orderNumber: 1,
  };

  const el = {
    categoryList: document.getElementById('category-list'),
    currentCategoryLabel: document.getElementById('current-category-label'),
    searchInput: document.getElementById('search-input'),
    menuGrid: document.getElementById('menu-grid'),
    orderItems: document.getElementById('order-items'),
    orderEmpty: document.getElementById('order-empty'),
    summarySubtotal: document.getElementById('summary-subtotal'),
    summaryTax: document.getElementById('summary-tax'),
    summaryTotal: document.getElementById('summary-total'),
    clearOrderBtn: document.getElementById('clear-order-btn'),
    btnOrder: document.getElementById('btn-order'),
    orderNumber: document.getElementById('order-number'),
    toast: document.getElementById('toast'),
  };

  function formatYen(value) {
    return '¥' + value.toLocaleString('ja-JP');
  }

  function calcSummary() {
    const subtotal = state.orderItems.reduce((sum, item) => sum + item.price * item.qty, 0);
    const tax = Math.floor(subtotal * TAX_RATE);
    const total = subtotal + tax;
    return { subtotal, tax, total };
  }

  function nextOrderNumber() {
    state.orderNumber += 1;
    if (state.orderNumber > 9999) state.orderNumber = 1;
    el.orderNumber.textContent = String(state.orderNumber).padStart(4, '0');
  }

  function showToast(message) {
    el.toast.textContent = message;
    el.toast.classList.remove('hidden');
    el.toast.classList.add('visible');
    setTimeout(() => {
      el.toast.classList.remove('visible');
      setTimeout(() => el.toast.classList.add('hidden'), 200);
    }, 1800);
  }

  function renderCategories() {
    el.categoryList.innerHTML = '';
    CATEGORIES.forEach((c) => {
      const count = c.id === 'ALL' ? MENU_ITEMS.length : MENU_ITEMS.filter((m) => m.category === c.id).length;
      const btn = document.createElement('button');
      btn.className = 'category-button' + (state.currentCategory === c.id ? ' active' : '');
      btn.dataset.category = c.id;
      btn.innerHTML = `
        <span>${c.icon} ${c.label}</span>
        <span class="badge">${count}</span>
      `;
      btn.addEventListener('click', () => {
        state.currentCategory = c.id;
        el.currentCategoryLabel.textContent = c.id === 'ALL' ? 'すべての商品' : c.label;
        renderCategories();
        renderMenu();
      });
      el.categoryList.appendChild(btn);
    });
  }

  function filterMenuItems() {
    return MENU_ITEMS.filter((item) => {
      if (state.currentCategory !== 'ALL' && item.category !== state.currentCategory) return false;
      if (state.search) {
        const q = state.search.toLowerCase();
        return item.name.toLowerCase().includes(q) || item.id.toLowerCase().includes(q);
      }
      return true;
    });
  }

  function renderMenu() {
    const items = filterMenuItems();
    el.menuGrid.innerHTML = '';
    items.forEach((item) => {
      const card = document.createElement('button');
      card.className = 'menu-card';
      card.innerHTML = `
        <div class="menu-card-header">
          <span class="menu-name">${item.name}</span>
          <span class="menu-code">${item.id}</span>
        </div>
        <div class="menu-footer">
          <span class="menu-price">${formatYen(item.price)}</span>
          <span class="menu-tag">${item.tag}</span>
        </div>
      `;
      card.addEventListener('click', () => {
        addToOrder(item.id);
      });
      el.menuGrid.appendChild(card);
    });
  }

  function addToOrder(itemId) {
    const menu = MENU_ITEMS.find((m) => m.id === itemId);
    if (!menu) return;
    const existing = state.orderItems.find((i) => i.id === itemId);
    if (existing) {
      existing.qty += 1;
    } else {
      state.orderItems.push({ ...menu, qty: 1 });
    }
    renderOrder();
    showToast(`${menu.name} を追加しました`);
  }

  function changeQty(itemId, delta) {
    const item = state.orderItems.find((i) => i.id === itemId);
    if (!item) return;
    item.qty += delta;
    if (item.qty <= 0) {
      state.orderItems = state.orderItems.filter((i) => i.id !== itemId);
    }
    renderOrder();
  }

  function removeItem(itemId) {
    const item = state.orderItems.find((i) => i.id === itemId);
    state.orderItems = state.orderItems.filter((i) => i.id !== itemId);
    renderOrder();
    if (item) showToast(`${item.name} を削除しました`);
  }

  function clearOrder() {
    state.orderItems = [];
    renderOrder();
  }

  function renderOrder() {
    el.orderItems.innerHTML = '';
    if (state.orderItems.length === 0) {
      el.orderEmpty.style.display = 'block';
    } else {
      el.orderEmpty.style.display = 'none';
    }

    state.orderItems.forEach((item) => {
      const tr = document.createElement('tr');
      tr.className = 'order-row';
      const line = item.price * item.qty;
      tr.innerHTML = `
        <td class="order-name">${item.name}</td>
        <td>
          <div class="order-qty">
            <button class="qty-btn" data-action="dec">-</button>
            <span class="qty-value">${item.qty}</span>
            <button class="qty-btn" data-action="inc">+</button>
          </div>
        </td>
        <td>${formatYen(item.price)}</td>
        <td>${formatYen(line)}</td>
        <td><button class="order-remove-button">削除</button></td>
      `;
      tr.querySelector('[data-action="inc"]').addEventListener('click', () => changeQty(item.id, 1));
      tr.querySelector('[data-action="dec"]').addEventListener('click', () => changeQty(item.id, -1));
      tr.querySelector('.order-remove-button').addEventListener('click', () => removeItem(item.id));
      el.orderItems.appendChild(tr);
    });

    const { subtotal, tax, total } = calcSummary();
    el.summarySubtotal.textContent = formatYen(subtotal);
    el.summaryTax.textContent = formatYen(tax);
    el.summaryTotal.textContent = formatYen(total);

    el.btnOrder.disabled = total === 0;
  }

  async function placeOrder() {
    const { subtotal, tax, total } = calcSummary();
    if (!total) return;
    el.btnOrder.disabled = true;
    const orderNumber = String(state.orderNumber).padStart(4, '0');
    const payload = {
      orderNumber,
      items: state.orderItems.map(i => ({ id: i.id, name: i.name, price: i.price, qty: i.qty })),
      subtotal,
      tax,
      total,
    };
    try {
      const res = await fetch('/api/orders', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload),
      });
      if (!res.ok) throw new Error('server error');
      showToast(`注文を送信しました（#${orderNumber}）`);
      clearOrder();
      nextOrderNumber();
    } catch {
      showToast('注文の送信に失敗しました');
      el.btnOrder.disabled = false;
    }
  }

  function bindEvents() {
    el.searchInput.addEventListener('input', (e) => {
      state.search = e.target.value.trim();
      renderMenu();
    });

    el.clearOrderBtn.addEventListener('click', () => {
      if (state.orderItems.length === 0) return;
      clearOrder();
      el.paymentReceived.textContent = formatYen(0);
      el.paymentChange.textContent = formatYen(0);
      state.keypadValue = '';
      showToast('注文をクリアしました');
    });

    el.btnOrder.addEventListener('click', () => placeOrder());
  }

  function init() {
    el.orderNumber.textContent = String(state.orderNumber).padStart(4, '0');
    renderCategories();
    renderMenu();
    renderOrder();
    bindEvents();
  }

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', init);
  } else {
    init();
  }
})();

