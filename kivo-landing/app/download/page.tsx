import FooterBackground from '../footer-background';

export default function Download() {
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
        
        <h1 className="page-title">download kivo</h1>
        <p style={{ fontFamily: 'Epilogue, Arial, sans-serif', fontSize: '1.2vw', marginBottom: '3vw', maxWidth: '40vw' }}>
          Ready to tell your computer simply what to do? Download Kivo and boost your workflow today.
        </p>
        
        <div className="download-layout">
          <a href="/kivo-setup.exe" download className="download-btn">
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" style={{ marginRight: '10px' }}>
              <rect x="2" y="3" width="20" height="14" rx="2" ry="2"></rect><line x1="8" y1="21" x2="16" y2="21"></line><line x1="12" y1="17" x2="12" y2="21"></line>
            </svg>
            Download for Windows
          </a>
          <span className="download-btn disabled" aria-disabled="true">
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" style={{ marginRight: '10px' }}>
              <path d="M12 20.94c1.5 0 2.75 1.06 4 1.06 3 0 6-8 6-12.22A4.91 4.91 0 0 0 17 5c-2.22 0-4 1.44-5 1.44C9.54 6.44 7.78 5 5.56 5 2 5 0 8 0 12c0 4.22 3 12.22 6 12.22 1.25 0 2.5-1.06 4-1.06Z"></path>
              <path d="M10 2c1 .5 2 2 2 5"></path>
            </svg>
            Mac (Coming Soon)
          </span>
        </div>
      </div>
    </main>
  );
}
