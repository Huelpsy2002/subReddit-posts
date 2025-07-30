import React, { useState } from "react";
import { Link, useNavigate } from "react-router-dom";

import UserMenu from "./UserMenuInfo";



function Header(props) {

    const [inputValue, setInputValue] = useState("")
    const [userMenuDisplay, SetuserMenuDisplay] = useState(false)
    const navigate = useNavigate();

  async function logout() {
        
       let response = await fetch("/api/logout", {
            method: "POST", 
            credentials: "include" 
        })
        if(response.ok){
            navigate("/")
        }

    }

  async function saveSubReddit(inputValue){
    try {
        const response = await fetch('/api/saveLane', {
            method: 'POST',
            credentials: "include",
            headers: {
                "Content-Type": "application/json"
              },
            body: JSON.stringify({
                userId: props.user.id,
                laneName:inputValue
                
            })
        });
        let json = await response.json()
        
        if (response.status===400 || response.status===409) {
            props.setRequestState(json.message)
        }
        setTimeout(() => {
            props.setRequestState("") //clean the request state after 5 seconds
        }, 5000);
    
      
    
    } catch (error) {
        alert('Request failed:', error.message);
    }
    
  }






    return <header className="header">
        <i className="fab fa-reddit" style={{ fontSize: 40, color: "#FF4500" }}> Reddit</i>
        <div style={{ display: "flex" }}>
            <input onChange={(e) => setInputValue(e.target.value)}
                type="text" value={inputValue} name="subReddit" id="subReddit-input" placeholder="Add SubReddit" />
            <button onClick={async (e) => {
                props.setRequestState("Loading ...")
                await saveSubReddit(inputValue)
                props.setSubReddit(inputValue)
                setInputValue("")
            }
            } className="AddSubRedditButton">+</button>

        </div>
        <p className="RequestState">{props.requestState}</p>

        <div style={{ display: "flex", flexDirection: "row", gap: "5px", alignItems: "center" }} >
            <div
                onClick={() => window.location.href = `https://www.reddit.com${props.user.subreddit?.url}`}
                onMouseEnter={() => SetuserMenuDisplay(true)}
                onMouseLeave={() => SetuserMenuDisplay(false)}
                className="avatar">
                <img
                    src={props.user.icon_img}
                    alt="userImg"
                    loading="lazy"
                    onError={(e) => {
                        e.target.style.display = 'none';
                        e.target.nextSibling.style.display = 'flex';
                    }}
                />
                <i className="fas fa-user" style={{
                    display: 'none',
                    fontSize: '60%',
                    color: '#999'
                }}></i>
            </div>
            <p className="userName">{props.user.name}</p>
            <button onClick={logout} className="logout-btn" title="Log out" style={{marginLeft: '4px', padding: '4px 8px', minWidth: 'unset', fontSize: '1rem', display: 'flex', alignItems: 'center', justifyContent: 'center'}}>
                <i className="fas fa-sign-out-alt"></i>
            </button>
        </div>
        <div className={`userInfosMenu ${userMenuDisplay ? 'show' : ''}`} style={userMenuDisplay ? { display: "flex" } : { display: "none" }}>
            <UserMenu user={props.user} />
        </div>
    </header >

}



export default Header;
