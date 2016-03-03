# DynamicTranslator 
[![Build Status](https://travis-ci.org/osoykan/DynamicTranslator.svg?branch=master)](https://travis-ci.org/osoykan/DynamicTranslator) [![Issue Stats](http://issuestats.com/github/osoykan/dynamictranslator/badge/issue?style=flat)](http://issuestats.com/github/osoykan/dynamictranslator) [![Issue Stats](http://issuestats.com/github/osoykan/dynamictranslator/badge/pr?style=flat)](http://issuestats.com/github/osoykan/dynamictranslator) [![Coverage Status](https://coveralls.io/repos/osoykan/DynamicTranslator/badge.svg?branch=master&service=github)](https://coveralls.io/github/osoykan/DynamicTranslator?branch=master) [![GitHub issues](https://img.shields.io/github/issues/osoykan/DynamicTranslator.svg)](https://github.com/osoykan/DynamicTranslator/issues) [![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg)](https://raw.githubusercontent.com/osoykan/DynamicTranslator/master/LICENSE)
<a href="http://sourcebrowser.io/Browse/osoykan/DynamicTranslator"><img src="https://camo.githubusercontent.com/54520255524a72a04b0b20191e804f1360f85ab2/68747470733a2f2f696d672e736869656c64732e696f2f62616467652f42726f7773652d536f757263652d677265656e2e737667" alt="Source Browser" data-canonical-src="https://img.shields.io/badge/Browse-Source-green.svg" style="max-width:100%;"></a>
<a href="https://scan.coverity.com/projects/osoykan-dynamictranslator">
  <img alt="Coverity Scan Build Status"
       src="https://scan.coverity.com/projects/7147/badge.svg"/>
</a>

While you are reading a pdf or something, DynamicTranslator detects selected texts instantly, translates them according to your language choice.

###Latest News
DynamicTranslator detects your selected text where the current window/application you on and translates including words/sentences immediately.

#### *Instantly detect text implemented.
#### *Google Translate added.
#### *Language Detection implemented
#### *Bing Translate added (2016-02-29)
#### *Text to speech added (2016-03-03)


###In Turkish
Bilindiği gibi bazı sözlükler bize api sağlamıyor bu yüzden bu proje tamamiyle windows ortamında sağlıklı ve en hızlı şekilde anlık çeviriyi pop-up yaklaşımıyla çözmeye yönelik bir amaçla yazılmıştır. Bulunduğunuz pencere/pdf veya herhangi birşeyde gezinirken mouse ile seçtiğiniz text'i algılayıp sonrasında, sırasıyla Google Translate, Tureng, Yandex, SesliSozluk'e gidip bulduğu anlamları bir araya getirip size windows notification olarak sunmaktadır.

Seçilen metnin dilini algılar.

###Başvurulan sözlükler
        
##### Tureng
##### Yandex
##### Sesli Sozluk
##### Google Translate
##### Microsoft Bing Translate
        

### Project Information and The Goal
This project provides translation words or sentences while reading and working and any needed time. So, I'm using this while PDF Ebook reading mostly.Project is small but very useful (at least me :)) I hope this useful for you.

C# , WPF

This is a view while translating, the translating is showing via toast notification for translated words.
[![Dynamic Translator - How to use ?](http://cdn.makeagif.com/media/3-03-2016/pG9psp.gif)](https://www.youtube.com/watch?v=9rqX0aVCTKw)
### Using
It has an app.config like below. I didn't do any UI implementation yet, i think it's not necessary :), but you can do, then let's contribute !

```
<appSettings>
    <add key="LeftOffset" value="500" />
    <add key="TopOffset" value="15" />
    <add key="ApiKey" value="" />
    <add key="SearchableCharacterLimit" value="100" />
    <add key="MaxNotifications" value="4" />
    <add key="FromLanguage" value="English" />
    <add key="ToLanguage" value="Turkish" />
    <add key="ClientSettingsProvider.ServiceUri" value="" />
</appSettings>
  ```
  
  And now; You should go Yandex Api console and get a Translate Api Key and paste it  this section.
  ```
  <add key="ApiKey" value="YOURTRANSLATEAPIKEY" /> 
  ```
  
  And you can change any language which allowing by YANDEX Translate system on here.
  ```
    <add key="FromLanguage" value="English" />
    <add key="ToLanguage" value="Turkish" />
  ```
##Powered By

<p><a href="https://camo.githubusercontent.com/d94f160ac291837e52a5a9f0a56d0f087281460c/687474703a2f2f7777772e6a6574627261696e732e636f6d2f696d672f6c6f676f732f6c6f676f5f7265736861727065725f736d616c6c2e676966" target="_blank"><img src="https://camo.githubusercontent.com/d94f160ac291837e52a5a9f0a56d0f087281460c/687474703a2f2f7777772e6a6574627261696e732e636f6d2f696d672f6c6f676f732f6c6f676f5f7265736861727065725f736d616c6c2e676966" width="142" height="29" alt="ReSharper" data-canonical-src="http://www.jetbrains.com/img/logos/logo_resharper_small.gif" style="max-width:100%;"></a></p>
  
#Implemented C# 6.0 and .NET 4.6
