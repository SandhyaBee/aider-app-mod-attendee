import React, { useState, useEffect } from 'react';

const LatencyWidget = () => {
    const [latency, setLatency] = useState(0);
    const [region, setRegion] = useState("Checking...");
    const [status, setStatus] = useState("neutral"); // neutral, warning, success

    useEffect(() => {
        const checkLatency = async () => {
            const start = Date.now();
            try {
                // Hitting a lightweight endpoint on the backend
                const response = await fetch('/api/health'); 
                const end = Date.now();
                const duration = end - start;
                setLatency(duration);

                // Important
                const serverRegion = response.headers.get('X-Azure-Region') || 'East US (Legacy)';
                setRegion(serverRegion);

                if (duration < 80) setStatus("success");
                else if (duration < 200) setStatus("warning");
                else setStatus("critical");

            } catch (e) {
                setLatency(999);
                setRegion("Offline");
                setStatus("critical");
            }
        };

        const interval = setInterval(checkLatency, 2000);
        return () => clearInterval(interval);
    }, []);

    const getColor = () => {
        if (status === 'success') return '#4ade80'; // Green
        if (status === 'warning') return '#facc15'; // Yellow
        return '#f87171'; // Red
    };

    return (
        <div style={{
            position: 'fixed',
            bottom: '20px',
            right: '20px',
            background: '#1e293b',
            padding: '15px',
            borderRadius: '12px',
            border: `2px solid ${getColor()}`,
            color: 'white',
            fontFamily: 'monospace',
            boxShadow: '0 4px 6px -1px rgba(0, 0, 0, 0.1)'
        }}>
            <div style={{ fontSize: '0.8rem', opacity: 0.8 }}>SYSTEM METRICS</div>
            <div style={{ fontSize: '1.2rem', fontWeight: 'bold' }}>{latency}ms</div>
            <div style={{ fontSize: '0.9rem', color: getColor() }}>● {region}</div>
        </div>
    );
};

export default LatencyWidget;