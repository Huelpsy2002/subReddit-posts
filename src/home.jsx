import React,{useState} from "react";
import Header from "./components/Header"
import Main from "./components/main"




function Home(props){
    const [subReddit, setSubReddit] = useState("")
    const [requestState, setRequestState] = useState("")

    return <>
     <Header setRequestState={setRequestState} requestState={requestState} setSubReddit={setSubReddit} user = {props.user} />
     <Main setRequestState={setRequestState} subReddit={subReddit} setSubReddit={setSubReddit} /></>
}


export default Home