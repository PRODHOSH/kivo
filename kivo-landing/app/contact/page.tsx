import FooterBackground from '../footer-background';

export default function Contact() {
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
        
        <h1 className="page-title">contact</h1>
        
        <div className="contact-layout">
          <div className="avatar-container">
            <img 
              src="https://github.com/prodhosh.png" 
              alt="Prodhosh Avatar" 
              className="contact-avatar"
            />
          </div>
          
          <div className="contact-info">
            <a href="mailto:hello@prodhosh.me" className="contact-item">
              <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"></path><polyline points="22,6 12,13 2,6"></polyline></svg>
              hello@prodhosh.me
            </a>
            <a href="https://github.com/prodhosh" target="_blank" rel="noopener noreferrer" className="contact-item">
              <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><path d="M9 19c-5 1.5-5-2.5-7-3m14 6v-3.87a3.37 3.37 0 0 0-.94-2.61c3.14-.35 6.44-1.54 6.44-7A5.44 5.44 0 0 0 20 4.77 5.07 5.07 0 0 0 19.91 1S18.73.65 16 2.48a13.38 13.38 0 0 0-7 0C6.27.65 5.09 1 5.09 1A5.07 5.07 0 0 0 5 4.77a5.44 5.44 0 0 0-1.5 3.78c0 5.42 3.3 6.61 6.44 7A3.37 3.37 0 0 0 9 18.13V22"></path></svg>
              github.com/prodhosh
            </a>
            <a href="https://linkedin.com/in/prodhoshvs" target="_blank" rel="noopener noreferrer" className="contact-item">
              <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><path d="M16 8a6 6 0 0 1 6 6v7h-4v-7a2 2 0 0 0-2-2 2 2 0 0 0-2 2v7h-4v-7a6 6 0 0 1 6-6z"></path><rect x="2" y="9" width="4" height="12"></rect><circle cx="4" cy="4" r="2"></circle></svg>
              linkedin.com/in/prodhoshvs
            </a>
            <a href="https://prodhosh.me" target="_blank" rel="noopener noreferrer" className="contact-item">
              <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><circle cx="12" cy="12" r="10"></circle><line x1="2" y1="12" x2="22" y2="12"></line><path d="M12 2a15.3 15.3 0 0 1 4 10 15.3 15.3 0 0 1-4 10 15.3 15.3 0 0 1-4-10 15.3 15.3 0 0 1 4-10z"></path></svg>
              prodhosh.me
            </a>
          </div>
        </div>
      </div>
    </main>
  );
}
