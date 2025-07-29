import React from "react";




function UserMenu(props) {
    return (
        <div>
            <div className="menuUserHeader">
                <div className="menuAvatar">
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
                <div className="menuUserDetails">
                    <h4 className="menuUserName">{props.user.name}</h4>
                    <p className="menuUserKarma">Karma: {props.user.total_karma || 0}</p>
                </div>
            </div>

            <div className="menuDivider"></div>

            <div className="menuUserStats">
                <div className="statItem">
                    <i className="fas fa-calendar-alt"></i>
                    <span>Member since {new Date(props.user.created_utc * 1000).toLocaleDateString()}</span>
                </div>
                <div className="statItem">
                    <i className="fas fa-trophy"></i>
                    <span>Gold: {props.user.gold_creddits || 0}</span>
                </div>
                <div className="statItem">
                    <i className="fas fa-eye"></i>
                    <span>Link Karma: {props.user.link_karma || 0}</span>
                </div>
                <div className="statItem">
                    <i className="fas fa-comment"></i>
                    <span>Comment Karma: {props.user.comment_karma || 0}</span>
                </div>
            </div>

            
        </div>
    );
}


export default UserMenu