import React from "react";
const clientID = import.meta.env.VITE_REDDIT_CLIENT_ID
const redierctUrl = import.meta.env.VITE_REDDIT_REDIRECT_URI
const state = import.meta.env.VITE_REDDIT_STATE





function Login() {


  const redditUrl = `https://www.reddit.com/api/v1/authorize?client_id=${clientID}&response_type=code&state=${state}&redirect_uri=${redierctUrl}&duration=permanent&scope=read+vote+identity`
  return <div className="login-container">
    <div className="login-box">
      <div className="login-title">Sign in to Reddit</div>
      <div className="login-desc">To continue, please log in with your Reddit account.</div>
      <button className="reddit-login-btn" onClick={() => { window.location.href = redditUrl }}>
        <img className="reddit-logo" src="https://www.redditstatic.com/desktop2x/img/favicon/apple-icon-180x180.png" alt="Reddit Logo" />
        Log in with Reddit
      </button>
    </div>
  </div>
}



export default Login