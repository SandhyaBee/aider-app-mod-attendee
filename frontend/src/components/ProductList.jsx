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
        .then(() => {
            window.dispatchEvent(new Event('cartUpdated'));
        });
    };

    if (loading) return <div className="loader">Accessing Central Archive...</div>;

    return (
        <div className="product-grid">
            {products.map(p => (
                <div key={p.id} className="product-card">
                    <div className="category-badge">{p.category}</div>
                    <h3>{p.name}</h3>
                    <p className="description">{p.description}</p>
                    <div className="tags-container">
                        {p.tags.map(tag => (
                            <span key={tag} className="tag">#{tag}</span>
                        ))}
                    </div>
                    <div className="price-row">
                        <span className="price">${p.price.toFixed(2)}</span>
                        <button onClick={() => addToCart(p)}>Add to Cart</button>
                    </div>
                </div>
            ))}
        </div>
    );
}

export default ProductList;