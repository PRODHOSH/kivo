"use client";

import { useState } from 'react';
import FooterBackground from './footer-background';
import BrandLogo from './brand-logo';

export default function Home() {
  const [activeView, setActiveView] = useState<'home' | 'features' | 'contact' | 'download'>('home');

  return (
    <main className="inner-page">
      <FooterBackground />
      <div className="inner-content">
        <div className="logo" role="img" aria-label="Studio logo" onClick={() => setActiveView('home')} style={{ cursor: 'pointer' }}>
          <BrandLogo />
        </div>
        
        {activeView === 'home' && (
          <>
            <div className="jobs">
              <span className="tag">want to work faster?</span>
              <span className="headline job-title">tell your computer<br />simply</span>
              <div className="footer-nav">
                <span onClick={() => setActiveView('features')} style={{ textDecoration: 'underline', cursor: 'pointer' }}>Features</span>
                <a href="https://github.com/PRODHOSH/kivo" target="_blank" rel="noopener noreferrer" style={{ textDecoration: 'underline' }}>Open Source (GitHub)</a>
                <span onClick={() => setActiveView('contact')} style={{ textDecoration: 'underline', cursor: 'pointer' }}>Contact</span>
              </div>
            </div>
            
            <div className="contact">
              <span className="tag">get kivo</span>
              <span onClick={() => setActiveView('download')} className="headline job-title" style={{ textDecoration: 'underline', cursor: 'pointer' }}>Download for Windows</span>
              <span className="headline job-title">boost your workflow*</span>
              <p className="note">*kivo automates the boring stuff. you do the creative stuff.</p>
              <div className="socials">
                <a href="https://linkedin.com/in/prodhoshvs" target="_blank" rel="noopener noreferrer" aria-label="LinkedIn">
                  <svg width="35" height="35" viewBox="0 0 24 24" fill="currentColor"><path d="M19 0h-14c-2.761 0-5 2.239-5 5v14c0 2.761 2.239 5 5 5h14c2.762 0 5-2.239 5-5v-14c0-2.761-2.238-5-5-5zm-11 19h-3v-11h3v11zm-1.5-12.268c-.966 0-1.75-.79-1.75-1.764s.784-1.764 1.75-1.764 1.75.79 1.75 1.764-.783 1.764-1.75 1.764zm13.5 12.268h-3v-5.604c0-3.368-4-3.113-4 0v5.604h-3v-11h3v1.765c1.396-2.586 7-2.777 7 2.476v6.759z"/></svg>
                </a>
                <a href="https://github.com/prodhosh" target="_blank" rel="noopener noreferrer" aria-label="GitHub">
                  <svg width="35" height="35" viewBox="0 0 24 24" fill="currentColor"><path d="M12 0c-6.626 0-12 5.373-12 12 0 5.302 3.438 9.8 8.207 11.387.599.111.793-.261.793-.577v-2.234c-3.338.726-4.033-1.416-4.033-1.416-.546-1.387-1.333-1.756-1.333-1.756-1.089-.745.083-.729.083-.729 1.205.084 1.839 1.237 1.839 1.237 1.07 1.834 2.807 1.304 3.492.997.107-.775.418-1.305.762-1.604-2.665-.305-5.467-1.334-5.467-5.931 0-1.311.469-2.381 1.236-3.221-.124-.303-.535-1.524.117-3.176 0 0 1.008-.322 3.301 1.23.957-.266 1.983-.399 3.003-.404 1.02.005 2.047.138 3.006.404 2.291-1.552 3.297-1.23 3.297-1.23.653 1.653.242 2.874.118 3.176.77.84 1.235 1.911 1.235 3.221 0 4.609-2.807 5.624-5.479 5.921.43.372.823 1.102.823 2.222v3.293c0 .319.192.694.801.576 4.765-1.589 8.199-6.086 8.199-11.386 0-6.627-5.373-12-12-12z"/></svg>
                </a>
                <a href="mailto:hello@prodhosh.me" aria-label="Email">
                  <svg width="35" height="35" viewBox="0 0 24 24" fill="currentColor"><path d="M0 3v18h24v-18h-24zm6.623 7.929l-4.623 5.712v-9.458l4.623 3.746zm-4.141-5.929h19.035l-9.517 7.713-9.518-7.713zm5.694 7.188l3.824 3.099 3.83-3.104 5.612 6.817h-18.779l5.513-6.812zm9.208-1.264l4.616-3.741v9.348l-4.616-5.607z"/></svg>
                </a>
              </div>
            </div>
          </>
        )}

        {activeView === 'features' && (
          <div style={{ flex: 1, display: 'flex', flexDirection: 'column' }}>
            <span onClick={() => setActiveView('home')} className="back-link" style={{ cursor: 'pointer' }}>
              <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" style={{ marginRight: '8px' }}>
                <path d="M19 12H5M12 19l-7-7 7-7" />
              </svg>
              Back to Home
            </span>
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
        )}

        {activeView === 'contact' && (
          <div style={{ flex: 1, display: 'flex', flexDirection: 'column' }}>
            <span onClick={() => setActiveView('home')} className="back-link" style={{ cursor: 'pointer' }}>
              <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" style={{ marginRight: '8px' }}>
                <path d="M19 12H5M12 19l-7-7 7-7" />
              </svg>
              Back to Home
            </span>
            <h1 className="page-title">contact</h1>
            <div className="contact-layout">
              <div className="avatar-container">
                <img src="https://github.com/prodhosh.png" alt="Prodhosh" className="contact-avatar" />
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
        )}

        {activeView === 'download' && (
          <div style={{ flex: 1, display: 'flex', flexDirection: 'column' }}>
            <span onClick={() => setActiveView('home')} className="back-link" style={{ cursor: 'pointer' }}>
              <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" style={{ marginRight: '8px' }}>
                <path d="M19 12H5M12 19l-7-7 7-7" />
              </svg>
              Back to Home
            </span>
            <h1 className="page-title">download</h1>
            <div className="download-layout" style={{ marginTop: '4vw', display: 'flex', gap: '3vw', alignItems: 'center', justifyContent: 'center' }}>
              <a href="https://github.com/PRODHOSH/kivo/releases/latest" className="download-btn">
                <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><path d="M2 12h20"></path><path d="M12 2v20"></path><path d="M4.93 4.93l14.14 14.14"></path><path d="M4.93 19.07L19.07 4.93"></path></svg>
                Download for Windows
              </a>
              <span className="download-btn disabled" aria-disabled="true" title="Coming soon">
                <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><path d="M22 12h-4l-3 9L9 3l-3 9H2"></path></svg>
                Download for Mac (Soon)
              </span>
            </div>
          </div>
        )}

      </div>
    </main>
  );
}
