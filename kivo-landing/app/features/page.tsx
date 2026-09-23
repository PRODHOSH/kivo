import FooterBackground from '../footer-background';

export default function Features() {
  return (
    <main className="inner-page">
      <FooterBackground />
      <div className="inner-content">
        <a href="/" className="back-link">
          <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" style={{ marginRight: '8px' }}>
            <path d="M19 12H5M12 19l-7-7 7-7" />
          </svg>
          Back to Home
        </a>
        
        <h1 className="page-title">features</h1>
        
        <div className="features-grid">
          <div className="feature-card">
            <div className="feature-icon">
              <svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><path d="M12 2a3 3 0 0 0-3 3v7a3 3 0 0 0 6 0V5a3 3 0 0 0-3-3Z"></path><path d="M19 10v2a7 7 0 0 1-14 0v-2"></path><line x1="12" y1="19" x2="12" y2="22"></line></svg>
            </div>
            <h3 className="feature-title">Local Voice Processing</h3>
            <p className="feature-desc">Powered by Whisper.net, your voice is transcribed instantly on your machine. Zero latency, 100% privacy.</p>
          </div>
          
          <div className="feature-card">
            <div className="feature-icon">
              <svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><polygon points="13 2 3 14 12 14 11 22 21 10 12 10 13 2"></polygon></svg>
            </div>
            <h3 className="feature-title">Smart Actions</h3>
            <p className="feature-desc">Kivo understands natural language intent and seamlessly automates your desktop workflows and apps.</p>
          </div>
          
          <div className="feature-card">
            <div className="feature-icon">
              <svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><rect x="2" y="3" width="20" height="14" rx="2" ry="2"></rect><line x1="8" y1="21" x2="16" y2="21"></line><line x1="12" y1="17" x2="12" y2="21"></line></svg>
            </div>
            <h3 className="feature-title">Sleek Overlay</h3>
            <p className="feature-desc">A beautiful, unobtrusive glassmorphism widget that lives on your screen only when you need it.</p>
          </div>

          <div className="feature-card">
            <div className="feature-icon">
              <svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><polygon points="13 2 3 14 12 14 11 22 21 10 12 10 13 2"></polygon></svg>
            </div>
            <h3 className="feature-title">Lightning Fast</h3>
            <p className="feature-desc">Direct OS integration means Kivo executes commands instantly without relying on slow cloud APIs.</p>
          </div>
        </div>
      </div>
    </main>
  );
}
