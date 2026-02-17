import { useState, useEffect } from 'react';

function Cart({ sessionId }) {
    const [cartItems, setCartItems] = useState([]);

    const loadCart = () => {
        if (!sessionId) return;
        fetch(`/api/carts/${sessionId}`)
            .then(res => res.json())
            .then(data => setCartItems(data))
            .catch(err => console.error("Failed to load cart", err));
    };

    useEffect(() => {
        loadCart();
        // Listen for updates from ProductList
        window.addEventListener('cartUpdated', loadCart);
        return () => window.removeEventListener('cartUpdated', loadCart);
    }, [sessionId]);

    const removeFromCart = (id) => {
        fetch(`/api/carts/${id}`, { method: 'DELETE' })
            .then(() => loadCart());
    };

    const total = cartItems.reduce((sum, item) => sum + (item.product ? item.product.price * item.quantity : 0), 0);

    return (
        <div className="cart-container">
            {cartItems.length === 0 ? (
                <p>Your cart is empty.</p>
            ) : (
                <>
                    <ul className="cart-list">
                        {cartItems.map(item => (
                            <li key={item.id} className="cart-item">
                                <div>
                                    <strong>{item.product?.name || 'Unknown Item'}</strong>
                                    <br/>
                                    <small>Qty: {item.quantity}</small>
                                </div>
                                <div>
                                    ${(item.product?.price * item.quantity).toFixed(2)}
                                    <button className="remove-btn" onClick={() => removeFromCart(item.id)}>×</button>
                                </div>
                            </li>
                        ))}
                    </ul>
                    <div className="cart-total">
                        <strong>Total: ${total.toFixed(2)}</strong>
                        <button className="checkout-btn">Checkout</button>
                    </div>
                </>
            )}
        </div>
    );
}

export default Cart;