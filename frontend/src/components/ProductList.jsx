import { useState, useEffect } from 'react';

function ProductList({ sessionId }) {
    const [products, setProducts] = useState([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        fetch('/api/products')
            .then(res => res.json())
            .then(data => {
                setProducts(data);
                setLoading(false);
            })
            .catch(err => console.error("Failed to load products", err));
    }, []);

    const addToCart = (product) => {
        fetch('/api/carts', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                sessionId: sessionId,
                productId: product.id,
                quantity: 1
            })
        })
        .then(res => {
            if(res.ok) {
                // Trigger a custom event to notify the Cart component to refresh
                window.dispatchEvent(new Event('cartUpdated'));
                alert(`Added ${product.name} to cart!`);
            }
        });
    };

    if (loading) return <div>Loading Catalog...</div>;

    return (
        <div className="product-grid">
            {products.map(p => (
                <div key={p.id} className="product-card">
                    <div className="product-image-placeholder">{p.category}</div>
                    <h3>{p.name}</h3>
                    <p>{p.description}</p>
                    <div className="price-row">
                        <span className="price">${p.price}</span>
                        <button onClick={() => addToCart(p)}>Add to Cart</button>
                    </div>
                </div>
            ))}
        </div>
    );
}

export default ProductList;