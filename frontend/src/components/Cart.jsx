import { useState, useEffect } from 'react';

function Cart({ sessionId }) {
    const [cartItems, setCartItems] = useState([]);

    const loadCart = () => {
        fetch(`/api/carts/${sessionId}`)
            .then(res => res.json())
            .then(data => setCartItems(data));
    };

    useEffect(() => {
        loadCart();
        window.addEventListener('cartUpdated', loadCart);
        return () => window.removeEventListener('cartUpdated', loadCart);
    }, [sessionId]);

    const handleCheckout = () => {
        const total = cartItems.reduce((sum, i) => sum + (i.product.price * i.quantity), 0);
        
        const order = {
            customerName: sessionId, // Use sessionId so the backend can find the right cart
            email: "hacker@styleverse.com",
            totalAmount: total,
            shippingRegion: "East US",
            orderItems: cartItems.map(i => ({
                productId: i.productId,
                quantity: i.quantity,
                unitPrice: i.product.price
            }))
        };

        fetch('/api/orders', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(order)
        }).then(res => {
            if (res.ok) {
                alert("Order Placed! Your SQL cart has been cleared.");
                // REFRESH THE UI
                window.dispatchEvent(new Event('cartUpdated')); 
            }
        });
    };

    return (
        <div className="cart-container">
            {cartItems.length === 0 ? <p>Cart empty.</p> : (
                <>
                    {cartItems.map(item => (
                        <div key={item.id} className="cart-item">
                            <span>{item.product.name} (x{item.quantity})</span>
                            <span>${(item.product.price * item.quantity).toFixed(2)}</span>
                        </div>
                    ))}
                    <button className="checkout-btn" onClick={handleCheckout}>
                        Secure Checkout (SQL)
                    </button>
                </>
            )}
        </div>
    );
}

export default Cart;